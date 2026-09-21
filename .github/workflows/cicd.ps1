param (
    [string]$GitHubToken,
    [string]$NuGetApiKey,
    [string]$IntTestNuGetApiKey,
    [string]$PowerShellGalleryApiKey
)

# Fail-fast defaults for reliable CI/local runs:
# - StrictMode 3: treat uninitialized variables, unknown members, etc. as errors.
# - ErrorActionPreference='Stop': convert non-terminating errors into terminating ones (catchable).
# Error-handling guidance:
# - In catch{ }, prefer Write-Error or 'throw' to preserve fail-fast behavior.
#   * Write-Error (with ErrorActionPreference='Stop') is terminating and bubbles to the caller 'throw' is always terminating and keeps stack context.
# - Using Write-Host in catch{ } only logs and SWALLOWS the exception; execution continues, use a sentinel value (e.g., $null) explicitly.
# - Note: native tool exit codes on PS5 aren’t governed by ErrorActionPreference; use the Invoke-Exec or Invoke-ProcessTyped wrapper to enforce policy.
Set-StrictMode -Version 3
$ErrorActionPreference     = 'Stop'   # errors become terminating
$Global:ConsoleLogMinLevel = 'INF'    # gate: TRC/DBG/INF/WRN/ERR/FTL

# Keep this script compatible with PowerShell 5.1 and PowerShell 7+
# Lean, pipeline-friendly style—simple, readable, and easy to modify, failfast on errors.
Write-Host "Powershell script $(Split-Path -Leaf $PSCommandPath) has started."

# Provides lightweight reachability guards for external services.
# Detection only—no installs, imports, network changes, or pushes. (e.g Test-PSGalleryConnectivity)
# Designed to short-circuit local and CI/CD workflows when dependencies are offline (e.g., skip a push if the Git host is unreachable).
. "$PSScriptRoot\cicd.bootstrap.ps1"

$PowerShellGalleryAvailable = Test-PSGalleryConnectivity
$null = Test-GitHubConnectivity

# Module installation depends on PSGallery only; GitHub connectivity is diagnostic here.
if ($PowerShellGalleryAvailable)
{
    Update-ModuleIfNeeded2 -ModuleName 'Eigenverft.Manifested.Drydock'
}

# A freshly installed module is not guaranteed to auto-load in the same process.
Import-Module -Name 'Eigenverft.Manifested.Drydock' -Force -ErrorAction Stop
$null = Test-ModuleAvailable -Name 'Eigenverft.Manifested.Drydock' -IncludePrerelease -ExitIfNotFound -Quiet

function Restore-DotnetProjectProperties {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo]$ProjectFileInfo,

        [Parameter(Mandatory = $true)]
        [string[]]$CommonArguments,

        [Parameter(Mandatory = $true)]
        [object]$GeneratedVersion,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$GeneratedVersionSuffix
    )

    # This is the first regular restore in the restore-clean-restore sequence. It also
    # evaluates the project state required by the remaining linear processing steps.
    $ProjectPropertyOutput = Invoke-ProcessTyped `
        -Executable 'dotnet' `
        -Arguments @(
            'restore',
            $ProjectFileInfo.FullName,
            '-nologo',
            '-p:Stage=restore',
            '-getProperty:UsingMicrosoftNETSdk',
            '-getProperty:TargetFrameworkVersion',
            '-getProperty:TargetFramework',
            '-getProperty:TargetFrameworks',
            '-getProperty:IsTestProject',
            '-getProperty:IsPackable',
            '-getProperty:IsPublishable',
            '-getProperty:IsCicdEnabled',
            '-getProperty:GitVersionBaseDirectory'
        ) `
        -CommonArguments $CommonArguments `
        -ReturnType Text `
        -CaptureOutput $true

    try
    {
        $EvaluatedProperties = ($ProjectPropertyOutput | ConvertFrom-Json -ErrorAction Stop).Properties
    }
    catch
    {
        throw "dotnet restore returned invalid project properties for '$($ProjectFileInfo.FullName)': $($_.Exception.Message)"
    }

    $IsSDKProj = [string]::Equals([string]$EvaluatedProperties.UsingMicrosoftNETSdk, 'true', [System.StringComparison]::OrdinalIgnoreCase)
    $IsNoneSDKProj = -not $IsSDKProj

    $TargetFrameworks = @()
    if (-not [string]::IsNullOrWhiteSpace([string]$EvaluatedProperties.TargetFramework))
    {
        $TargetFrameworks = @([string]$EvaluatedProperties.TargetFramework)
    }
    elseif (-not [string]::IsNullOrWhiteSpace([string]$EvaluatedProperties.TargetFrameworks))
    {
        $TargetFrameworks = @(([string]$EvaluatedProperties.TargetFrameworks).Split(';') | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    }

    $IsSDKWithFramework = $false
    foreach ($TargetFramework in $TargetFrameworks)
    {
        if ($TargetFramework.ToLowerInvariant() -in @('net20', 'net35', 'net40', 'net403', 'net45', 'net451', 'net452', 'net46', 'net461', 'net462', 'net47', 'net471', 'net472', 'net48', 'net481'))
        {
            $IsSDKWithFramework = $true
            break
        }
    }

    $IsTestProject = $IsSDKProj -and [string]::Equals([string]$EvaluatedProperties.IsTestProject, 'true', [System.StringComparison]::OrdinalIgnoreCase)
    $IsPackable = $IsSDKProj -and [string]::Equals([string]$EvaluatedProperties.IsPackable, 'true', [System.StringComparison]::OrdinalIgnoreCase)
    $IsPublishable = $IsSDKProj -and [string]::Equals([string]$EvaluatedProperties.IsPublishable, 'true', [System.StringComparison]::OrdinalIgnoreCase)
    $IsCicdEnabled = -not [string]::Equals([string]$EvaluatedProperties.IsCicdEnabled, 'false', [System.StringComparison]::OrdinalIgnoreCase)

    # A configured GitVersionBaseDirectory is the opt-in marker used by the repository templates.
    $GitVersionBaseDirectory = [string]$EvaluatedProperties.GitVersionBaseDirectory
    $UsesNerdbankGitVersioning = -not [string]::IsNullOrWhiteSpace(([string]$GitVersionBaseDirectory).Trim())

    if ($UsesNerdbankGitVersioning)
    {
        # GetBuildVersion is supplied by Nerdbank.GitVersioning. No TargetFramework override
        # is used because that would reduce a multi-target project's project.assets.json.
        $VersionOutput = Invoke-ProcessTyped `
            -Executable 'dotnet' `
            -Arguments @(
                'restore',
                $ProjectFileInfo.FullName,
                '-nologo',
                '-t:GetBuildVersion',
                '-getProperty:NuGetPackageVersion',
                '-getProperty:AssemblyInformationalVersion',
                '-getProperty:BuildVersionSimple'
            ) `
            -CommonArguments $CommonArguments `
            -ReturnType Text `
            -CaptureOutput $true

        try
        {
            $VersionProperties = ($VersionOutput | ConvertFrom-Json -ErrorAction Stop).Properties
        }
        catch
        {
            throw "Nerdbank.GitVersioning returned invalid version data for '$($ProjectFileInfo.FullName)': $($_.Exception.Message)"
        }

        $NuGetPackageVersion = [string]$VersionProperties.NuGetPackageVersion
        if ([string]::IsNullOrWhiteSpace($NuGetPackageVersion))
        {
            throw "Nerdbank.GitVersioning did not calculate NuGetPackageVersion for '$($ProjectFileInfo.FullName)'."
        }

        $VersionSource = 'Nerdbank.GitVersioning'
        $OutputVersion = $NuGetPackageVersion
        $AssemblyInformationalVersion = [string]$VersionProperties.AssemblyInformationalVersion
        $BuildVersionSimple = [string]$VersionProperties.BuildVersionSimple
    }
    else
    {
        $VersionSource = 'GeneratedVersion'
        $OutputVersion = [string]$GeneratedVersion.VersionFull
        $NuGetPackageVersion = $OutputVersion
        if (-not [string]::IsNullOrWhiteSpace($GeneratedVersionSuffix))
        {
            $NuGetPackageVersion = "$NuGetPackageVersion$GeneratedVersionSuffix"
        }
        $AssemblyInformationalVersion = $NuGetPackageVersion
        $BuildVersionSimple = $OutputVersion
    }

    return [pscustomobject]@{
        IsSDKProj                     = $IsSDKProj
        IsNoneSDKProj                 = $IsNoneSDKProj
        IsSDKWithFramework            = $IsSDKWithFramework
        TargetFrameworkVersion        = [string]$EvaluatedProperties.TargetFrameworkVersion
        TargetFrameworks              = $TargetFrameworks
        IsTestProject                 = $IsTestProject
        IsPackable                    = $IsPackable
        IsPublishable                 = $IsPublishable
        IsCicdEnabled                 = $IsCicdEnabled
        Source                        = $VersionSource
        UsesNerdbankGitVersioning     = $UsesNerdbankGitVersioning
        OutputVersion                 = $OutputVersion
        NuGetPackageVersion           = $NuGetPackageVersion
        AssemblyInformationalVersion  = $AssemblyInformationalVersion
        BuildVersionSimple            = $BuildVersionSimple
    }
}

# Required for updating PowerShellGet and PackageManagement providers in local PowerShell 5.x environments
Initialize-PowerShellMiniBootstrap

# Test TLS, NuGet, PackageManagement, PowerShellGet, and PSGallery publish endpoint
Test-PsGalleryPublishPrereqsOffline -ExitOnFailure

# Clean up previous versions of the module to avoid conflicts in local PowerShell environments
Uninstall-PreviousModuleVersions -ModuleName 'Eigenverft.Manifested.Drydock'

# Verify required commands are available, even a windows update could remove them temporarily
$null = Test-CommandAvailable -Command "dotnet" -ExitIfNotFound
$null = Test-CommandAvailable -Command "git" -ExitIfNotFound

# In the case the secrets are not passed as parameters, try to get them from the secrets file, local development or CI/CD environment
Test-VariableValue -Variable { $GitHubToken } -WarnIfNullOrEmpty -HideValue
Test-VariableValue -Variable { $NuGetApiKey } -WarnIfNullOrEmpty -HideValue
Test-VariableValue -Variable { $IntTestNuGetApiKey } -WarnIfNullOrEmpty -HideValue
Test-VariableValue -Variable { $PowerShellGalleryApiKey } -WarnIfNullOrEmpty -HideValue
$GitHubToken = Get-ConfigValue -Check $GitHubToken -FilePath (Join-Path $PSScriptRoot 'cicd.secrets.json') -Property 'GitHubToken'
$NuGetApiKey = Get-ConfigValue -Check $NuGetApiKey -FilePath (Join-Path $PSScriptRoot 'cicd.secrets.json') -Property 'NuGetApiKey'
$IntTestNuGetApiKey = Get-ConfigValue -Check $IntTestNuGetApiKey -FilePath (Join-Path $PSScriptRoot 'cicd.secrets.json') -Property 'IntTestNuGetApiKey'
$PowerShellGalleryApiKey = Get-ConfigValue -Check $PowerShellGalleryApiKey -FilePath (Join-Path $PSScriptRoot 'cicd.secrets.json') -Property 'PowerShellGalleryApiKey'
Test-VariableValue -Variable { $GitHubToken } -ExitIfNullOrEmpty -HideValue
Test-VariableValue -Variable { $NuGetApiKey } -ExitIfNullOrEmpty -HideValue
Test-VariableValue -Variable { $IntTestNuGetApiKey } -ExitIfNullOrEmpty -HideValue
Test-VariableValue -Variable { $PowerShellGalleryApiKey } -ExitIfNullOrEmpty -HideValue

# Preload environment information
$RunEnvironment = Get-RunEnvironment
$GitRepositoryRoot = Get-GitTopLevelDirectory
$GitCurrentBranch = Get-GitCurrentBranch
$GitBranchRootDirectory = Get-GitCurrentBranchRoot
$GitRepositoryName = Get-GitRepositoryName
$GitRemoteUrl = Get-GitRemoteUrl

# Failfast / guard if any of the required preloaded environment information is not available
Test-VariableValue -Variable { $RunEnvironment } -ExitIfNullOrEmpty
Test-VariableValue -Variable { $GitRepositoryRoot } -ExitIfNullOrEmpty
Test-VariableValue -Variable { $GitCurrentBranch } -ExitIfNullOrEmpty
Test-VariableValue -Variable { $GitBranchRootDirectory } -ExitIfNullOrEmpty
Test-VariableValue -Variable { $GitRepositoryName } -ExitIfNullOrEmpty
Test-VariableValue -Variable { $GitRemoteUrl } -ExitIfNullOrEmpty

# Generate deployment info based on the current branch name
$BranchDeploymentConfig = Convert-BranchToDeploymentInfo -BranchName "$GitCurrentBranch"

# Generates a version based on the current date time to verify the version functions work as expected
$GeneratedVersion = Convert-DateTimeTo64SecVersionComponents -VersionBuild 1 -VersionMajor 0
#$GeneratedVersion.VersionFull = "0.1.20256.30636"
$GeneratedVersionAsDateTime = Convert-64SecVersionComponentsToDateTime -VersionBuild $GeneratedVersion.VersionBuild -VersionMajor $GeneratedVersion.VersionMajor -VersionMinor $GeneratedVersion.VersionMinor -VersionRevision $GeneratedVersion.VersionRevision
Test-VariableValue -Variable { $GeneratedVersion } -ExitIfNullOrEmpty
Test-VariableValue -Variable { $GeneratedVersionAsDateTime } -ExitIfNullOrEmpty

# Generate a local PowerShell Gallery repository to publish to.
$LocalPowerShellGalleryName = "LocalPowerShellGallery"
$LocalPowerShellGalleryName = Register-LocalPSGalleryRepository -RepositoryName "$LocalPowerShellGalleryName"

# Generate a local NuGet package source to publish to.
$LocalNuGetSourceName = "LocalNuGet"
$LocalNuGetSourceName = Register-LocalNuGetDotNetPackageSource -SourceName "$LocalNuGetSourceName"

# All config files paths
$ConfigRootPath = Get-Path -Paths @($PSScriptRoot,".config")

$SPDXCachePath = Get-Path -Paths @("$ConfigRootPath","SPDX_cache")
$DotNetToolsManifestPath = Get-Path -Paths @("$ConfigRootPath","dotnet-tools","dotnet-tools.json")
$NuGetAllowedLicensesPath = Get-Path -Paths @("$ConfigRootPath","nuget-license","allowed-licenses.json")
$NuGetLicenseMappingsPath = Get-Path -Paths @("$ConfigRootPath","nuget-license","licenses-mapping.json")
$NuGetLicenseFileMappingsPath = Get-Path -Paths @("$ConfigRootPath","nuget-license","license-file-mappings.json")

# Enable github specific nuget sources.
$GitHubPackagesUser = "carsten-riedel"
$GitHubSourceName = "github"
$GitHubSourceUri = "https://nuget.pkg.github.com/$GitHubPackagesUser/index.json"
$NuGetTestSourceUri = "https://apiint.nugettest.org/v3/index.json"
$NuGetOrgSourceUri = "https://api.nuget.org/v3/index.json"
Unregister-LocalNuGetDotNetPackageSource -SourceName "$GitHubSourceName"

# Enable the .NET tools specified in the manifest file
Enable-TempDotnetTools -ManifestFile "$DotNetToolsManifestPath" -NoReturn

# Required output root folder
$OutputRootPath = Get-Path -Paths @("$GitRepositoryRoot","output")
New-Directory -Paths @($OutputRootPath)

# Delete clean the outputfolder
if (-not $($RunEnvironment.IsCI)) { Remove-FilesByPattern -Path "$OutputRootPath" -Pattern "*"  }

# Drops are disposable run output. Clear them before processing so local runs do not
# retain obsolete aggregate versions and failed runs cannot leave a mixed snapshot.
$Drop = "C:\temp\$GitRepositoryName-drops"
New-Directory -Paths @($Drop)
Remove-FilesByPattern -Path "$Drop" -Pattern "*"

# The generated version remains available for legacy projects and aggregate file drops.
$ChannelLatestRelativePath = Get-Path -Paths @($BranchDeploymentConfig.Channel.Value,"latest")

# All required output folders
$BuildRootPath = Get-Path -Paths @("$OutputRootPath","build")
$BuildBinPath = Get-Path -Paths @("$BuildRootPath","bin")
$BuildObjPath = Get-Path -Paths @("$BuildRootPath","obj")

$PackRootPath = Get-Path -Paths @("$OutputRootPath","pack")
$PublishRootPath = Get-Path -Paths @("$OutputRootPath","publish")
$RepoPublishRootPath = Get-Path -Paths @("$OutputRootPath","publish_repo")
$SlnPublishRootPath = Get-Path -Paths @("$OutputRootPath","publish_sln")
$ProjPublishRootPath = Get-Path -Paths @("$OutputRootPath","publish_proj")
$ReportsRootPath =  Get-Path -Paths @("$OutputRootPath","reports")


# Main pipeline preparation: discover every solution below src and resolve its projects.
# The resulting solution-to-project execution plan drives all subsequent build, test,
# pack, publish, reporting, and distribution stages.
$SolutionFileInfos = Find-FilesByPattern -Path "$GitRepositoryRoot\src" -Pattern "*.sln;*.slnx"
$SolutionProjectPaths = @()
foreach ($solutionFile in $SolutionFileInfos) {
    # Drydock returns the project paths in their deterministic execution order.
    $CurrentProjectPaths = Invoke-ProcessTyped -Executable "drydock.exe" -Arguments @( "sln", "--location", "$($solutionFile.FullName)") -ReturnType 'Objects'
    $SolutionProjectPaths += [pscustomobject]@{ Sln =$solutionFile; Prj = ($CurrentProjectPaths | ForEach-Object { Get-Item $_ }) };
}

$Vswhere = Find-FilesByPattern -Path "${env:ProgramFiles(x86)}\Microsoft Visual Studio" -Pattern "vswhere.exe"
$MsBuildVs = Invoke-ProcessTyped -Executable "$($Vswhere.FullName)" -Arguments @("-latest", "-products","*", "-requires","Microsoft.Component.MSBuild", "-find", "**\Bin\MSBuild.exe") -ReturnType Objects

# Build, Test, Pack, Publish, and Generate Reports for each project in the solution.
$ProjectVersionInfos = @{}
foreach ($SolutionProjectPath in $SolutionProjectPaths) {
    foreach ($ProjectFileInfo in $SolutionProjectPath.Prj) {

        $DotnetCommonParameters = @(
            "-p:Configuration=Release",
            "-p:Platform=AnyCPU",
            "-v:minimal",
            "-p:Deterministic=true",
            "-p:ContinuousIntegrationBuild=true",
            "-p:UseSharedCompilation=false",
            "-m:1"
        )

        $GeneratedVersionParameters = @(
            "-p:VersionBuild=$($GeneratedVersion.VersionBuild)",
            "-p:VersionMajor=$($GeneratedVersion.VersionMajor)",
            "-p:VersionMinor=$($GeneratedVersion.VersionMinor)",
            "-p:VersionRevision=$($GeneratedVersion.VersionRevision)",
            "-p:VersionSuffix=$($BranchDeploymentConfig.Affix.Suffix)"
        )

        # Determine the project state and perform the first restore.
        $ProjectProperties = Restore-DotnetProjectProperties -ProjectFileInfo $ProjectFileInfo -CommonArguments $DotnetCommonParameters -GeneratedVersion $GeneratedVersion -GeneratedVersionSuffix ([string]$BranchDeploymentConfig.Affix.Suffix)

        $ProjectVersionParameters = @()
        if ((-not $ProjectProperties.UsesNerdbankGitVersioning) -and (-not $ProjectProperties.IsTestProject))
        {
            $ProjectVersionParameters = $GeneratedVersionParameters
        }

        $BuildPackPublishParameters = @($DotnetCommonParameters + $ProjectVersionParameters)

        $ProjectVersionInfos[$ProjectFileInfo.FullName] = $ProjectProperties
        Write-Output "Version for '$($ProjectFileInfo.BaseName)': $($ProjectProperties.NuGetPackageVersion) ($($ProjectProperties.Source))."

        if (-not $ProjectProperties.IsCicdEnabled)
        {
            Write-Output "Skipping '$($ProjectFileInfo.BaseName)' because IsCicdEnabled is false."
            continue
        }

        $ProjectChannelVersionRelativePath = Get-Path -Paths @($BranchDeploymentConfig.Channel.Value,$ProjectProperties.OutputVersion)

        # Prepare legacy output paths and parameters before executing build steps.
        if ($ProjectProperties.IsNoneSDKProj)
        {
            $ProjectBranchVersionRelativePath = Get-Path -Paths @($BranchDeploymentConfig.Branch.PathSegmentsSanitized,$ProjectProperties.OutputVersion)
            New-Directory -Paths @($BuildRootPath)
            $BuildBinDirectory = New-Directory -Paths @($BuildBinPath,$SolutionProjectPath.Sln.BaseName,$ProjectFileInfo.BaseName,$ProjectBranchVersionRelativePath)
            $BuildObjDirectory = New-Directory -Paths @($BuildObjPath,$SolutionProjectPath.Sln.BaseName,$ProjectFileInfo.BaseName,$ProjectBranchVersionRelativePath)
            $NonSDKParameters = @(
                "-p:Configuration=Release",
                "-p:Platform=AnyCPU",
                "-v:minimal",
                $ProjectVersionParameters
                "-p:OutputPath=$($BuildBinDirectory)/",
                "-p:BaseIntermediateOutputPath=$($BuildObjDirectory)/",
                "-p:UseSharedCompilation=false"
            )
        }

        # The version-resolution step performed the first restore. Complete the established
        # restore-clean-restore sequence for a predictable incremental build state.
        Invoke-ProcessTyped -Executable "dotnet" -Arguments @("clean", "$($ProjectFileInfo.FullName)", "-p:Stage=clean") -ReturnType Objects -CommonArguments $DotnetCommonParameters
        Invoke-ProcessTyped -Executable "dotnet" -Arguments @("restore", "$($ProjectFileInfo.FullName)", "-p:Stage=restore") -ReturnType Objects -CommonArguments $DotnetCommonParameters

        # Build Non-SDK projects with Visual Studio MSBuild.
        if ($ProjectProperties.IsNoneSDKProj)
        {
            Invoke-ProcessTyped -Executable "$MsBuildVs" -Arguments @("$($ProjectFileInfo.FullName)", "-p:Stage=build") -CommonArguments $NonSDKParameters -ReturnType Objects -CaptureOutput $true -CaptureOutputDump $false
        }

        #SDK style build with frameworks requires msbuild to build the project, otherwise dotnet build is used for SDK style projects without frameworks
        if ($ProjectProperties.IsSDKProj)
        {
            if ($ProjectProperties.IsSDKWithFramework)
            {
                Invoke-ProcessTyped -Executable "$MsBuildVs" -Arguments @("/t:Build","$($ProjectFileInfo.FullName)", "-p:Stage=build")  -CommonArguments $BuildPackPublishParameters -ReturnType Objects -CaptureOutput $true -CaptureOutputDump $false
            }
            else {
                Invoke-ProcessTyped -Executable "dotnet" -Arguments @("build","$($ProjectFileInfo.FullName)", "-p:Stage=build")  -CommonArguments $BuildPackPublishParameters -ReturnType Objects -CaptureOutput $true -CaptureOutputDump $false
            }
        }

        # Report generation and license validation is only relevant for packable or publishable projects. Reports are generated after the build step to ensure the project.assets.json file is available for analysis.
        if (($ProjectProperties.IsPackable -eq $true) -or ($ProjectProperties.IsPublishable -eq $true))
        {
            $ReportsDirectory = New-Directory -Paths @($ReportsRootPath,$SolutionProjectPath.Sln.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)

            #Dependency-Health-and-Inventory.Report
            $VulnerabilitiesJson = Invoke-ProcessTyped -Executable "dotnet" -Arguments @("list", "$($ProjectFileInfo.FullName)", "package", "--vulnerable", "--format", "json")
            New-DotnetVulnerabilitiesReport -jsonInput $VulnerabilitiesJson -OutputFile "$ReportsDirectory\Vulnerabilities.md" -OutputFormat markdown -ExitOnVulnerability $false

            $DeprecatedPackagesJson = Invoke-ProcessTyped -Executable "dotnet" -Arguments @("list", "$($ProjectFileInfo.FullName)", "package", "--deprecated", "--include-transitive", "--format", "json")
            New-DotnetDeprecatedReport -jsonInput $DeprecatedPackagesJson -OutputFile "$ReportsDirectory\Deprecated.md" -OutputFormat markdown -IgnoreTransitivePackages $true -ExitOnDeprecated $false

            $BillOfMaterialsJson = Invoke-ProcessTyped -Executable "dotnet" -Arguments @("list", "$($ProjectFileInfo.FullName)", "package", "--include-transitive", "--format", "json")
            New-DotnetBillOfMaterialsReport -jsonInput $BillOfMaterialsJson -OutputFile "$ReportsDirectory\BillOfMaterials.md" -OutputFormat markdown -IgnoreTransitivePackages $true

            Join-FileText -InputFiles @("$ReportsDirectory\BillOfMaterials.md", "$ReportsDirectory\Vulnerabilities.md","$ReportsDirectory\Deprecated.md") -OutputFile "$ReportsDirectory\BOM\SBOM-$(($ProjectFileInfo.BaseName).Replace('.','_')).md" -BetweenFiles 'One' -CreateOutputDirectory

            $NuGetLicenseReportPath = "$ReportsDirectory/$($ProjectFileInfo.BaseName).ThirdPartyLicencesNotices.json"
            Invoke-ProcessTyped -Executable "nuget-license" -Arguments @("--input", "$($ProjectFileInfo.FullName)", "--allowed-license-types", "$NuGetAllowedLicensesPath", "--output", "JsonPretty", "--licenseurl-to-license-mappings", "$NuGetLicenseMappingsPath", "--licensefile-to-license-mappings", "$NuGetLicenseFileMappingsPath", "--file-output", "$NuGetLicenseReportPath" ) -AllowedExitCodes @(0,1)
            $NuGetLicenseExitCode = $LASTEXITCODE
            if ($NuGetLicenseExitCode -ne 0)
            {
                if (Test-Path -LiteralPath $NuGetLicenseReportPath -PathType Leaf)
                {
                    Write-Host "nuget-license validation report:"
                    Get-Content -LiteralPath $NuGetLicenseReportPath -Raw | Write-Host
                }
                throw "nuget-license found disallowed or unresolved package licenses (exit code $NuGetLicenseExitCode)."
            }

            Export-PackageLicenseTexts -JsonPath "$ReportsDirectory/$($ProjectFileInfo.BaseName).ThirdPartyLicencesNotices.json" -OutputDirectory "$ReportsDirectory\licenses" -CacheDirectory "$SPDXCachePath"
        }

        # Test only executes for SDK-style projects. Non-SDK projects are not supported by dotnet test.
        if ($ProjectProperties.IsTestProject -eq $true)
        {
            Invoke-ProcessTyped -Executable "dotnet" -Arguments @("test", "$($ProjectFileInfo.FullName)", "-c", "Release", '-p:Stage=test' ) -CommonArguments $DotnetCommonParameters -CaptureOutput $false
        }

        if ($ProjectProperties.IsPackable -eq $true)
        {
            $PackDirectory = New-Directory -Paths @($PackRootPath,$SolutionProjectPath.Sln.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            Invoke-ProcessTyped -Executable "dotnet" -Arguments @("pack", "$($ProjectFileInfo.FullName)", "-c", "Release","-p:""Stage=pack""","-p:""PackageOutputPath=$($PackDirectory)""")  -CommonArguments $BuildPackPublishParameters -CaptureOutput $false
        }

        if ($ProjectProperties.IsPublishable -eq $true)
        {
            $PublishDirectory = New-Directory -Paths @($PublishRootPath,$SolutionProjectPath.Sln.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            Invoke-ProcessTyped -Executable "dotnet" -Arguments @("publish", "$($ProjectFileInfo.FullName)", "-c", "Release","-p:""Stage=publish""","-p:""PublishDir=$($PublishDirectory)""")  -CommonArguments $BuildPackPublishParameters -CaptureOutput $false
        }

        if ($ProjectProperties.IsNoneSDKProj) {
            $PublishDirectory = New-Directory -Paths @($PublishRootPath,$SolutionProjectPath.Sln.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            Copy-FilesRecursively -SourceDirectory "$($BuildBinDirectory)" -DestinationDirectory "$($PublishDirectory)" -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
        }

    }
}

# Select projects for file distribution. Pack-only projects keep their NuGet packages
# and reports; they do not create publish trees or file drops.
$PublishSolutionPaths = @()
foreach ($SolutionProjectPath in $SolutionProjectPaths) {
    $PublishProjects = @($SolutionProjectPath.Prj | Where-Object {
        $Properties = $ProjectVersionInfos[$_.FullName]
        $Properties.IsCicdEnabled -and (-not $Properties.IsTestProject) -and
            ($Properties.IsPublishable -or $Properties.IsNoneSDKProj)
    })
    if ($PublishProjects.Count -gt 0) {
        $PublishSolutionPaths += [pscustomobject]@{ Sln = $SolutionProjectPath.Sln; Prj = $PublishProjects }
    }
}

# Enrich every project publish tree before creating distributable drops.
# A repository can contain multiple solutions and every solution can contain multiple projects.
# Their publish trees remain isolated as publish/<solution>/<project>/<channel>/<version>.
# Compliance files are copied next to the binaries.
foreach ($SolutionProjectPath in $PublishSolutionPaths) {
    foreach ($ProjectFileInfo in $SolutionProjectPath.Prj) {
        $SolutionFileInfo = $SolutionProjectPath.Sln
            $ProjectChannelVersionRelativePath = Get-Path -Paths @($BranchDeploymentConfig.Channel.Value,$ProjectVersionInfos[$ProjectFileInfo.FullName].OutputVersion)
            $PublishDirectory = New-Directory -Paths @($PublishRootPath,$SolutionFileInfo.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            $ReportsDirectory = New-Directory -Paths @($ReportsRootPath,$SolutionFileInfo.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            Copy-FilesRecursively -SourceDirectory "$ReportsDirectory" -DestinationDirectory "$PublishDirectory" -Filter "LICENSE-*" -CopyEmptyDirs $false -ForceOverwrite $true
            Copy-FilesRecursively -SourceDirectory "$ReportsDirectory" -DestinationDirectory "$PublishDirectory" -Filter "SBOM-*" -CopyEmptyDirs $false -ForceOverwrite $true
            Remove-EmptyDirectories -Path "$(Get-Path -Paths @($PublishRootPath,$SolutionFileInfo.BaseName))" -RemoveRootIfEmpty
            Remove-EmptyDirectories -Path "$(Get-Path -Paths @($ReportsRootPath,$SolutionFileInfo.BaseName))" -RemoveRootIfEmpty
     }
}

# Remove build-only symbol files from every enriched project publish tree.
# All repository-, solution-, and project-level drops below are created from these cleaned trees.
foreach ($SolutionProjectPath in $PublishSolutionPaths) {
    foreach ($ProjectFileInfo in $SolutionProjectPath.Prj) {
        $SolutionFileInfo = $SolutionProjectPath.Sln
            $ProjectChannelVersionRelativePath = Get-Path -Paths @($BranchDeploymentConfig.Channel.Value,$ProjectVersionInfos[$ProjectFileInfo.FullName].OutputVersion)
            $PublishDirectory = New-Directory -Paths @($PublishRootPath,$SolutionFileInfo.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            Remove-FilesByPattern -Path "$PublishDirectory" -Pattern "*.pdb"
            Remove-EmptyDirectories -Path "$(Get-Path -Paths @($PublishRootPath,$SolutionFileInfo.BaseName))" -RemoveRootIfEmpty
     }
}

# Every aggregation level below is exposed as:
# - <channel>/<version>: version-specific snapshot
# - <channel>/latest: refreshed copy of the latest version in that channel
# - distributed: refreshed channel-independent distribution
# - zipped/<name>.<version>-<affix>.zip: NuGet-style file name for a regular ZIP archive

# Build the repository-level all-in-one drop by flattening the publish trees of every
# project from every solution. Project output file names are therefore expected to be unique.
if ($PublishSolutionPaths.Count -gt 0) {
    $AggregateChannelVersionRelativePath = Get-Path -Paths @($BranchDeploymentConfig.Channel.Value,$GeneratedVersion.VersionFull)
    $RepoPublishDirectory = New-Directory -Paths @($RepoPublishRootPath,$AggregateChannelVersionRelativePath)
}

foreach ($SolutionProjectPath in $PublishSolutionPaths) {
    $SolutionFileInfo = $SolutionProjectPath.Sln
    foreach ($ProjectFileInfo in $SolutionProjectPath.Prj) {
            $ProjectChannelVersionRelativePath = Get-Path -Paths @($BranchDeploymentConfig.Channel.Value,$ProjectVersionInfos[$ProjectFileInfo.FullName].OutputVersion)
            $PublishDirectory = New-Directory -Paths @($PublishRootPath,$SolutionFileInfo.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            Copy-FilesRecursively -SourceDirectory "$PublishDirectory" -DestinationDirectory "$RepoPublishDirectory" -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true
            Remove-EmptyDirectories -Path "$(Get-Path -Paths @($PublishRootPath,$SolutionFileInfo.BaseName))" -RemoveRootIfEmpty
    }
}

### FILE DROP SECTION
$RepositoryDropRootPath = "$Drop\rep"
$SolutionsDropRootPath = "$Drop\sln"
$ProjectsDropRootPath = "$Drop\prj"

if ($PublishSolutionPaths.Count -gt 0) {
    Copy-FilesRecursively -SourceDirectory "$RepoPublishDirectory" -DestinationDirectory (Get-Path -Paths @($RepositoryDropRootPath,$GitRepositoryName,$AggregateChannelVersionRelativePath)) -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
    Copy-FilesRecursively -SourceDirectory "$RepoPublishDirectory" -DestinationDirectory (Get-Path -Paths @($RepositoryDropRootPath,$GitRepositoryName,$ChannelLatestRelativePath)) -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
    Copy-FilesRecursively -SourceDirectory "$RepoPublishDirectory" -DestinationDirectory (Get-Path -Paths @($RepositoryDropRootPath,$GitRepositoryName,"distributed")) -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
    $nugetFilePart1 = Join-Text -InputObject @("$($GitRepositoryName)","$($GeneratedVersion.VersionFull)") -Separator '.' -Normalization Trim
    $nugetFileEmulation = Join-Text -InputObject @("$nugetFilePart1","$($BranchDeploymentConfig.Affix.Label)") -Separator '-' -Normalization Trim
    Compress-Directory -SourceDirectory "$RepoPublishDirectory" -DestinationFile "$(Get-Path -Paths @($RepositoryDropRootPath,$GitRepositoryName,"zipped","$nugetFileEmulation.zip"))"
}

# Build one solution-level drop by flattening all project publish trees belonging to that
# solution. The solution staging directory is cleared first to prevent stale artifacts.
foreach ($SolutionProjectPath in $PublishSolutionPaths) {
    $SolutionFileInfo = $SolutionProjectPath.Sln
    $SolutionPublishDirectory = New-Directory -Paths @($SlnPublishRootPath,$SolutionFileInfo.BaseName,$AggregateChannelVersionRelativePath)
    Remove-FilesByPattern -Path "$SolutionPublishDirectory" -Pattern "*"
    foreach ($ProjectFileInfo in $SolutionProjectPath.Prj) {
            $ProjectChannelVersionRelativePath = Get-Path -Paths @($BranchDeploymentConfig.Channel.Value,$ProjectVersionInfos[$ProjectFileInfo.FullName].OutputVersion)
            $PublishDirectory = New-Directory -Paths @($PublishRootPath,$SolutionFileInfo.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            Copy-FilesRecursively -SourceDirectory "$PublishDirectory" -DestinationDirectory "$SolutionPublishDirectory" -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true
    }
    Copy-FilesRecursively -SourceDirectory "$SolutionPublishDirectory" -DestinationDirectory (Get-Path -Paths @($SolutionsDropRootPath,$SolutionFileInfo.BaseName,$AggregateChannelVersionRelativePath)) -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
    Copy-FilesRecursively -SourceDirectory "$SolutionPublishDirectory" -DestinationDirectory (Get-Path -Paths @($SolutionsDropRootPath,$SolutionFileInfo.BaseName,$ChannelLatestRelativePath)) -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
    Copy-FilesRecursively -SourceDirectory "$SolutionPublishDirectory" -DestinationDirectory (Get-Path -Paths @($SolutionsDropRootPath,$SolutionFileInfo.BaseName,"distributed")) -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
    $nugetFilePart1 = Join-Text -InputObject @("$($SolutionFileInfo.BaseName)","$($GeneratedVersion.VersionFull)") -Separator '.' -Normalization Trim
    $nugetFileEmulation = Join-Text -InputObject @("$nugetFilePart1","$($BranchDeploymentConfig.Affix.Label)") -Separator '-' -Normalization Trim
    Compress-Directory -SourceDirectory "$SolutionPublishDirectory" -DestinationFile "$(Get-Path -Paths @($SolutionsDropRootPath,$SolutionFileInfo.BaseName,"zipped","$nugetFileEmulation.zip"))"
}

# Build one project-level drop for every solution/project association.
# Project drops are keyed by project base name, which must be unique across the repository.
foreach ($SolutionProjectPath in $PublishSolutionPaths) {
    $SolutionFileInfo = $SolutionProjectPath.Sln
    foreach ($ProjectFileInfo in $SolutionProjectPath.Prj) {
            $ProjectVersionInfo = $ProjectVersionInfos[$ProjectFileInfo.FullName]
            $ProjectChannelVersionRelativePath = Get-Path -Paths @($BranchDeploymentConfig.Channel.Value,$ProjectVersionInfo.OutputVersion)
            $PublishDirectory = New-Directory -Paths @($PublishRootPath,$SolutionFileInfo.BaseName,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            $ProjPublishDirectory = New-Directory -Paths @($ProjPublishRootPath,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)
            Copy-FilesRecursively -SourceDirectory "$PublishDirectory" -DestinationDirectory "$ProjPublishDirectory" -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true
            Copy-FilesRecursively -SourceDirectory "$ProjPublishDirectory" -DestinationDirectory (Get-Path -Paths @($ProjectsDropRootPath,$ProjectFileInfo.BaseName,$ProjectChannelVersionRelativePath)) -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
            Copy-FilesRecursively -SourceDirectory "$ProjPublishDirectory" -DestinationDirectory (Get-Path -Paths @($ProjectsDropRootPath,$ProjectFileInfo.BaseName,$ChannelLatestRelativePath)) -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
            Copy-FilesRecursively -SourceDirectory "$ProjPublishDirectory" -DestinationDirectory (Get-Path -Paths @($ProjectsDropRootPath,$ProjectFileInfo.BaseName,"distributed")) -Filter "*" -CopyEmptyDirs $false -ForceOverwrite $true -CleanDestination MirrorTree
            $nugetFilePart1 = Join-Text -InputObject @("$($ProjectFileInfo.BaseName)","$($ProjectVersionInfo.OutputVersion)") -Separator '.' -Normalization Trim
            $nugetFileEmulation = Join-Text -InputObject @("$nugetFilePart1","$($BranchDeploymentConfig.Affix.Label)") -Separator '-' -Normalization Trim
            Compress-Directory -SourceDirectory "$ProjPublishDirectory" -DestinationFile "$(Get-Path -Paths @($ProjectsDropRootPath,$ProjectFileInfo.BaseName,"zipped","$nugetFileEmulation.zip"))"
    }
}

# Resolving deployment information for the current branch
$DeploymentChannel = $BranchDeploymentConfig.Channel.Value

$PushToLocalSource = $false
$PushToGitHubSource = $false
$PushToNuGetTest = $false
$PushToNuGetOrg = $false

# Determine where to publish based on the deployment channel
if ($DeploymentChannel -in @("development"))
{
    $PushToLocalSource = $true
    $PushToGitHubSource = $true
    $PushToNuGetTest = $false
    $PushToNuGetOrg = $false
}

if ($DeploymentChannel -in @('quality'))
{
    $PushToLocalSource = $true
    $PushToGitHubSource = $true
    $PushToNuGetTest = $true
    $PushToNuGetOrg = $false
}

if ($DeploymentChannel -in @('staging'))
{
    $PushToLocalSource = $true
    $PushToGitHubSource = $true
    $PushToNuGetTest = $true
    $PushToNuGetOrg = $false
}

if ($DeploymentChannel -in @('production'))
{
    $PushToLocalSource = $true
    $PushToGitHubSource = $true
    $PushToNuGetTest = $false
    $PushToNuGetOrg = $true
}

# Deploy *.nupkg artifacts to the appropriate destinations
if ($PushToLocalSource -eq $true)
{
    $NuGetPackageFileInfos = Find-FilesByPattern -Path "$PackRootPath" -Pattern "*.nupkg"
    foreach ($NuGetPackageFileInfo in $NuGetPackageFileInfos)
    {
        Invoke-ProcessTyped -Executable "dotnet" -Arguments @("nuget", "push", "$($NuGetPackageFileInfo.FullName)", "--source","$LocalNuGetSourceName")
    }
}

if ($PushToGitHubSource -eq $true)
{
    Invoke-ProcessTyped -Executable "dotnet" -Arguments @("nuget","add", "source", "--username", "$GitHubPackagesUser","--password","$GitHubToken","--store-password-in-clear-text","--name","$GitHubSourceName","$GitHubSourceUri") -CaptureOutput $false -CaptureOutputDump $false -HideValues @($GitHubToken)

    $NuGetPackageFileInfos = Find-FilesByPattern -Path "$PackRootPath" -Pattern "*.nupkg"
    foreach ($NuGetPackageFileInfo in $NuGetPackageFileInfos)
    {
        Invoke-ProcessTyped -Executable "dotnet" -Arguments @("nuget","push", "$($NuGetPackageFileInfo.FullName)", "--api-key", "$GitHubToken","--source","$GitHubSourceName","--skip-duplicate") -HideValues @($GitHubToken)
    }
    Unregister-LocalNuGetDotNetPackageSource -SourceName "$GitHubSourceName"
}

if ($PushToNuGetTest -eq $true)
{
    $NuGetPackageFileInfos = Find-FilesByPattern -Path "$PackRootPath" -Pattern "*.nupkg"
    foreach ($NuGetPackageFileInfo in $NuGetPackageFileInfos)
    {
        Invoke-ProcessTyped -Executable "dotnet" -Arguments @("nuget","push", "$($NuGetPackageFileInfo.FullName)", "--api-key", "$IntTestNuGetApiKey","--source","$NuGetTestSourceUri","--skip-duplicate") -HideValues @($IntTestNuGetApiKey)
    }
}

if ($PushToNuGetOrg -eq $true)
{
    $NuGetPackageFileInfos = Find-FilesByPattern -Path "$PackRootPath" -Pattern "*.nupkg"
    foreach ($NuGetPackageFileInfo in $NuGetPackageFileInfos)
    {
        Invoke-ProcessTyped -Executable "dotnet" -Arguments @("nuget","push", "$($NuGetPackageFileInfo.FullName)", "--api-key", "$NuGetApiKey","--source","$NuGetOrgSourceUri","--skip-duplicate") -HideValues @($NuGetApiKey)
    }
}
