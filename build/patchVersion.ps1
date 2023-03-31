# GITHUB_REF=refs/tags/<Tag>
# GITHUB_REF_NAME=<Tag>
# GITHUB_REF_TYPE=tag
# GITHUB_SHA=<SHA1>

if (!$env:GITHUB_REF.StartsWith('refs/tags/v')) {
    exit 1;
}

$version = $env:GITHUB_REF.trimStart('refs/tags/v')

$majorVersion = $version.split('.')[0]
$fileVersion = $majorVersion + ".0.0"

$filePath = './src/Directory.Build.props'
$content = Get-Content -path $filePath -raw -Encoding utf8
$content = $content -replace "<AssemblyVersion>0.0.0</AssemblyVersion>", "<AssemblyVersion>$version</AssemblyVersion>"
$content = $content -replace "<FileVersion>0.0.0</FileVersion>", "<FileVersion>$fileVersion</FileVersion>"
$content = $content -replace "<Version>0.0.0-dev</Version>", "<Version>$version</Version>"
Set-Content -path $filePath -Value $content -Encoding utf8

Write-Host "patched to $version"
