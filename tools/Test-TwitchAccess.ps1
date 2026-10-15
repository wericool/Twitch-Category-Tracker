param(
    [string[]]$Channels = @('shroud', 'twitchdev', 'tracker_missing_9f314a')
)

$ErrorActionPreference = 'Stop'
# Uses only public page content. No browser session, cookies, or user tokens.
$page = Invoke-WebRequest -Uri 'https://www.twitch.tv/shroud' -UseBasicParsing -TimeoutSec 25
$publicId = [regex]::Match($page.Content, 'clientId="([a-z0-9]+)"').Groups[1].Value
if (-not $publicId) { throw 'Public website client ID not found; Twitch page format may have changed.' }

foreach ($channel in $Channels) {
    if ($channel -notmatch '^[a-zA-Z0-9_]{1,25}$') { throw "Invalid channel login: $channel" }
    $body = @{
        query = 'query TrackerProbe($login: String!) { user(login: $login) { login displayName stream { id title game { id name } } broadcastSettings { title game { id name } } } }'
        variables = @{ login = $channel.ToLowerInvariant() }
    } | ConvertTo-Json -Depth 5 -Compress
    $response = Invoke-RestMethod -Uri 'https://gql.twitch.tv/gql' -Method Post -ContentType 'application/json' -Headers @{'Client-ID' = $publicId} -Body $body -TimeoutSec 25
    if ($response.errors) { throw ($response.errors | ConvertTo-Json -Depth 8 -Compress) }
    if (-not $response.PSObject.Properties['data'] -or -not $response.data.PSObject.Properties['user']) {
        throw 'Unexpected Twitch response shape.'
    }
    $user = $response.data.user
    $status = if ($null -eq $user) { 'not_found' } elseif ($null -eq $user.stream) { 'offline' } else { 'live' }
    [pscustomobject]@{
        checked_at_utc = [DateTime]::UtcNow.ToString('o')
        channel = $channel
        status = $status
        live_category = $user.stream.game.name
        configured_category = $user.broadcastSettings.game.name
        source = 'unofficial_twitch_website_graphql'
    }
}
