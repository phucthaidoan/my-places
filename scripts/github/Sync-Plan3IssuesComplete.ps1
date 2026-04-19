<#
.SYNOPSIS
    Đóng các GitHub issue MVP liên quan Plan 3 (Trips) sau khi triển khai xong.

.DESCRIPTION
    Mặc định đóng **#11–#14** trên `phucthaidoan/my-places` (`[Trips] …`) với
    `--reason completed` và comment đóng từ `bodies/plan3-issues-close-comment.md`.

    Cần **GitHub CLI** đã đăng nhập (`gh auth login`) — khuyến nghị.

    Nếu không có `gh`, script thử **GITHUB_TOKEN** (classic PAT, scope `repo`) qua REST API:
    POST comment rồi PATCH `state=closed` + `state_reason=completed`.

.EXAMPLE
    pwsh ./scripts/github/Sync-Plan3IssuesComplete.ps1 -DryRun

.EXAMPLE
    pwsh ./scripts/github/Sync-Plan3IssuesComplete.ps1 -AlsoCommentPlan4Issue
#>
param(
    [string] $Owner = "phucthaidoan",
    [string] $Repo = "my-places",
    [int[]] $IssueNumbers = @(11, 12, 13, 14),
    [switch] $AlsoCommentPlan4Issue,
    [int] $Plan4IssueNumber = 33,
    [switch] $DryRun
)

$ErrorActionPreference = "Stop"
$commentPath = Join-Path $PSScriptRoot "bodies/plan3-issues-close-comment.md"
$plan4CommentPath = Join-Path $PSScriptRoot "bodies/plan4-unblocked-by-plan3-comment.md"

if (-not (Test-Path $commentPath)) {
    Write-Error "Missing: $commentPath"
}

$comment = Get-Content -LiteralPath $commentPath -Raw -Encoding UTF8
$full = "$Owner/$Repo"

function Add-IssueCommentRest {
    param([int] $Number, [string] $Body)
    $token = $env:GITHUB_TOKEN
    if (-not $token) { throw "GITHUB_TOKEN not set" }
    $headers = @{
        Authorization          = "Bearer $token"
        Accept                 = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
    }
    $uri = "https://api.github.com/repos/$Owner/$Repo/issues/$Number/comments"
    $json = (@{ body = $Body } | ConvertTo-Json -Compress)
    Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $json -ContentType "application/json; charset=utf-8"
}

function Close-IssueRest {
    param([int] $Number)
    $token = $env:GITHUB_TOKEN
    if (-not $token) { throw "GITHUB_TOKEN not set" }
    $headers = @{
        Authorization          = "Bearer $token"
        Accept                 = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
    }
    $uri = "https://api.github.com/repos/$Owner/$Repo/issues/$Number"
    $json = (@{ state = "closed"; state_reason = "completed" } | ConvertTo-Json -Compress)
    Invoke-RestMethod -Uri $uri -Method Patch -Headers $headers -Body $json -ContentType "application/json; charset=utf-8"
}

foreach ($n in $IssueNumbers) {
    if ($DryRun) {
        Write-Host "DRY RUN: close #$n on $full (comment length: $($comment.Length))"
        continue
    }

    if (Get-Command gh -ErrorAction SilentlyContinue) {
        gh issue close $n -R $full --reason completed --comment $comment
        if ($LASTEXITCODE -ne 0) {
            Write-Error "gh issue close failed for #$n"
        }
    }
    elseif ($env:GITHUB_TOKEN) {
        Add-IssueCommentRest -Number $n -Body $comment
        Close-IssueRest -Number $n
    }
    else {
        Write-Error "Install GitHub CLI (winget install GitHub.cli) and run 'gh auth login', or set GITHUB_TOKEN (classic PAT with repo scope)."
    }

    Start-Sleep -Milliseconds 400
}

if ($AlsoCommentPlan4Issue) {
    if (-not (Test-Path $plan4CommentPath)) {
        Write-Warning "Skip Plan 4 comment: missing $plan4CommentPath"
    }
    elseif ($DryRun) {
        Write-Host "DRY RUN: add comment to #$Plan4IssueNumber"
    }
    elseif (Get-Command gh -ErrorAction SilentlyContinue) {
        $c = Get-Content -LiteralPath $plan4CommentPath -Raw -Encoding UTF8
        gh issue comment $Plan4IssueNumber -R $full --body $c
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "gh issue comment failed for #$Plan4IssueNumber"
        }
    }
    elseif ($env:GITHUB_TOKEN) {
        $c = Get-Content -LiteralPath $plan4CommentPath -Raw -Encoding UTF8
        Add-IssueCommentRest -Number $Plan4IssueNumber -Body $c
    }
}

Write-Host "Done. Target issues: $($IssueNumbers -join ', ') on $full"
