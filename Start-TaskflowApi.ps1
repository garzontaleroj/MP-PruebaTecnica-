param(
    [ValidateNotNullOrEmpty()]
    [string]$SqlUser = 'sa'
)

$ErrorActionPreference = 'Stop'
$password = Read-Host "Password de SQL Server para $SqlUser" -AsSecureString
if ($null -eq $password -or $password.Length -eq 0) {
    throw 'Debes ingresar la clave de SQL Server.'
}

$previousConnection = $env:ConnectionStrings__TaskManagement
try {
    $credential = [pscredential]::new($SqlUser, $password)
    $connection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $connection.set_ConnectionString('Server=tcp:localhost,1433;Database=TaskManagementDb;Encrypt=True;TrustServerCertificate=True')
    $connection.Add('User ID', $SqlUser)
    $connection.Add('Password', $credential.GetNetworkCredential().Password)
    $env:ConnectionStrings__TaskManagement = $connection.get_ConnectionString()
    $connection.Clear()

    dotnet run --project (Join-Path $PSScriptRoot 'backend\TaskManagement.Api')
    if ($LASTEXITCODE -ne 0) {
        throw "La API se cerro con el codigo $LASTEXITCODE."
    }
} finally {
    $env:ConnectionStrings__TaskManagement = $previousConnection
    $password.Dispose()
}
