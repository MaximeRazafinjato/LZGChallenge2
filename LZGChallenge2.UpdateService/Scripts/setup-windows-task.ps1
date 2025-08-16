# Script PowerShell pour configurer le service LZG Update en tant que tâche Windows
# Exécuter en tant qu'administrateur

param(
    [Parameter(Mandatory=$true)]
    [string]$ServicePath,
    
    [Parameter(Mandatory=$false)]
    [int]$IntervalMinutes = 5,
    
    [Parameter(Mandatory=$false)]
    [string]$TaskName = "LZG Challenge Update Service"
)

Write-Host "=== Configuration du Service LZG Challenge Update ===" -ForegroundColor Green

# Vérifier si le chemin existe
if (-not (Test-Path $ServicePath)) {
    Write-Error "Le chemin spécifié n'existe pas: $ServicePath"
    exit 1
}

# Vérifier si la tâche existe déjà
$existingTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($existingTask) {
    Write-Host "La tâche '$TaskName' existe déjà. Suppression..." -ForegroundColor Yellow
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
}

try {
    # Créer l'action de la tâche
    $dotnetPath = (Get-Command dotnet).Source
    $arguments = "`"$ServicePath\LZGChallenge2.UpdateService.dll`""
    
    $action = New-ScheduledTaskAction -Execute $dotnetPath -Argument $arguments -WorkingDirectory $ServicePath
    
    # Créer le déclencheur (répétition toutes les X minutes)
    $trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes $IntervalMinutes) -RepetitionDuration (New-TimeSpan -Days 365)
    
    # Configurer les paramètres de la tâche
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RunOnlyIfNetworkAvailable
    
    # Créer la tâche
    $task = New-ScheduledTask -Action $action -Trigger $trigger -Settings $settings -Description "Service de mise à jour automatique pour LZG Challenge - League of Legends SoloQ Competition"
    
    # Enregistrer la tâche (s'exécute sous le compte système)
    Register-ScheduledTask -TaskName $TaskName -InputObject $task -User "SYSTEM"
    
    Write-Host "✅ Tâche '$TaskName' créée avec succès!" -ForegroundColor Green
    Write-Host "📅 Intervalle: Toutes les $IntervalMinutes minutes" -ForegroundColor Cyan
    Write-Host "📂 Chemin: $ServicePath" -ForegroundColor Cyan
    Write-Host "🔧 Commande: $dotnetPath $arguments" -ForegroundColor Cyan
    
    # Démarrer la tâche immédiatement pour test
    Write-Host "`n🚀 Test d'exécution immédiate..." -ForegroundColor Yellow
    Start-ScheduledTask -TaskName $TaskName
    
    # Attendre un peu et vérifier le statut
    Start-Sleep -Seconds 3
    $taskInfo = Get-ScheduledTask -TaskName $TaskName
    $lastResult = (Get-ScheduledTaskInfo -TaskName $TaskName).LastTaskResult
    
    if ($lastResult -eq 0) {
        Write-Host "✅ Test réussi! Le service s'exécute correctement." -ForegroundColor Green
    } else {
        Write-Host "⚠️  Résultat du test: $lastResult (vérifiez les logs)" -ForegroundColor Yellow
    }
    
    Write-Host "`n📋 Commandes utiles:" -ForegroundColor Cyan
    Write-Host "   Vérifier le statut: Get-ScheduledTask -TaskName '$TaskName'" -ForegroundColor Gray
    Write-Host "   Voir les exécutions: Get-ScheduledTaskInfo -TaskName '$TaskName'" -ForegroundColor Gray
    Write-Host "   Exécuter manuellement: Start-ScheduledTask -TaskName '$TaskName'" -ForegroundColor Gray
    Write-Host "   Supprimer la tâche: Unregister-ScheduledTask -TaskName '$TaskName'" -ForegroundColor Gray
    
} catch {
    Write-Error "Erreur lors de la création de la tâche: $($_.Exception.Message)"
    exit 1
}

Write-Host "`n✅ Configuration terminée!" -ForegroundColor Green