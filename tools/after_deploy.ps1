$ErrorActionPreference = "Stop";
if ($env:TRAVIS_TAG -NotLike '*-*') 
{
  "It's not a prerelease tag. Mark release as not prerelease."
  "Getting release ID for tag $($env:TRAVIS_TAG)"
  $response = Invoke-WebRequest `
    -Uri "https://api.github.com/repos/$($env:TRAVIS_REPO_SLUG)/releases/tags/$($env:TRAVIS_TAG)"  `
    -Headers @{Authorization="token $($env:GITHUB_RELEASE_TOKEN)"} `
    | ConvertFrom-Json
  "Setting release as not prerelease"
  $patchResponse = Invoke-WebRequest `
    -Uri "https://api.github.com/repos/$($env:TRAVIS_REPO_SLUG)/releases/$($response.id)" `
    -Method PATCH -ContentType 'application/json' `
    -Headers @{Authorization="token $($env:GITHUB_RELEASE_TOKEN)"} `
    -Body '{"prerelease": false}'
  "Status code: $($patchResponse.StatusCode)"
}