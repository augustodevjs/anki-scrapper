# Passo a Passo para Executar o Anki Scraper em Segundo Plano no macOS

## 1. Compilar o Projeto

Compile o projeto em modo Release para gerar os binários otimizados:

```bash
cd /Users/contractor.jmonteiro/Documents/personal-projects/scraping-anki
dotnet build -c Release
```

## 2. Instalar o LaunchAgent

Copie o arquivo plist para a pasta LaunchAgents do usuário:

```bash
cp /Users/contractor.jmonteiro/Documents/personal-projects/scraping-anki/com.user.ankiscraper.plist ~/Library/LaunchAgents/
```

Defina as permissões corretas:

```bash
chmod 644 ~/Library/LaunchAgents/com.user.ankiscraper.plist
```

## 3. Carregar o Serviço no LaunchD

Carregue o serviço no launchd para iniciar a execução:

```bash
launchctl load ~/Library/LaunchAgents/com.user.ankiscraper.plist
```

## 4. Verificar Status do Serviço

Para confirmar que o serviço está em execução:

```bash
launchctl list | grep ankiscraper
```

Se estiver funcionando, você verá uma saída semelhante a:
```
-	78	com.user.ankiscraper
```

O número (78 neste exemplo) é o ID do processo.

## 5. Monitorar os Logs

Os logs do aplicativo são salvos em:

- Log principal: `/Users/contractor.jmonteiro/Documents/personal-projects/scraping-anki/logs/ankiscraper_app.log`
- Log de saída padrão: `/Users/contractor.jmonteiro/Documents/personal-projects/scraping-anki/logs/ankiscraper.log`
- Log de erros: `/Users/contractor.jmonteiro/Documents/personal-projects/scraping-anki/logs/ankiscraper_error.log`

Para visualizar os logs em tempo real:

```bash
tail -f /Users/contractor.jmonteiro/Documents/personal-projects/scraping-anki/logs/ankiscraper_app.log
```

## 6. Reiniciar o Serviço

Se você fizer alterações no código, recompile e reinicie o serviço:

```bash
dotnet build -c Release
launchctl unload ~/Library/LaunchAgents/com.user.ankiscraper.plist
launchctl load ~/Library/LaunchAgents/com.user.ankiscraper.plist
```

## 7. Parar o Serviço

Para interromper o serviço temporariamente:

```bash
launchctl stop com.user.ankiscraper
```

## 8. Desativar o Serviço

Para desativar o serviço permanentemente:

```bash
launchctl unload ~/Library/LaunchAgents/com.user.ankiscraper.plist
```

## 9. Remover o Serviço

Para remover completamente o serviço:

```bash
launchctl unload ~/Library/LaunchAgents/com.user.ankiscraper.plist
rm ~/Library/LaunchAgents/com.user.ankiscraper.plist
```