# Define the repo
$repo = "dotnet/runtime"

# Number of issues per page (max 100)
$perPage = 100

# GitHub API URL
$url = "https://api.github.com/repos/$repo/issues?state=open&per_page=$perPage"

# Set GitHub headers (User-Agent required)
$headers = @{ "User-Agent" = "dotnet-tsv-script" }

# Get the issues
$response = Invoke-RestMethod -Uri $url -Headers $headers

# Prepare TSV header
$tsv = "ID`tArea`tTitle"

# Convert to TSV rows
foreach ($issue in $response) {
    # Skip pull requests
    if ($issue.PSObject.Properties.Name -contains 'pull_request') { continue }

    # Default area
    $area = "Unlabeled"

    # Try to find area label
    if ($issue.labels -ne $null) {
        $areaLabel = ($issue.labels | Where-Object { $_.name -like "area-*" }) | Select-Object -First 1
        if ($areaLabel -ne $null) {
            $area = $areaLabel.name
        }
    }

    # Clean title
    $title = ($issue.title -replace "`t", " ") -replace "`r?`n", " "
    $id = $issue.number

    # Add line to TSV
    $tsv += "`n$id`t$area`t$title"
}

# Save TSV
$tsv | Set-Content -Path "Data/issues_train.tsv" -Encoding UTF8

Write-Host "✅ Saved to Data/issues_train.tsv"
