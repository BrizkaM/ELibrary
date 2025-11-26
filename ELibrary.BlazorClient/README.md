# E-Library Blazor Client

Blazor Server klientská aplikace pro správu knihovny.

## Obsah projektu

**ELibrary.BlazorClient** - Blazor Server aplikace (čistý UI klient bez business logiky)

## Před spuštěním

### Povolte CORS v API projektu

Do souboru `Program.cs` v API projektu přidejte CORS politiku před `var app = builder.Build();`:

```csharp
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins("https://localhost:7002", "http://localhost:5002")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
```

A po `var app = builder.Build();` přidejte:

```csharp
app.UseCors("AllowBlazorClient");
```

## Spuštění

### 1. Spuštění API

Nejprve spusťte API projekt:

```bash
cd ELibrary.WebApp
dotnet run
```

API by mělo běžet na `https://localhost:7001`

### 2. Spuštění Blazor klienta

V novém terminálu spusťte Blazor projekt:

```bash
cd ELibrary.BlazorClient
dotnet run
```

Blazor aplikace bude dostupná na `https://localhost:7002` (nebo jiném portu, který se zobrazí v konzoli)

### 3. Spuštění z Visual Studia

1. Otevřete solution ve Visual Studiu
2. Přidejte projekt `ELibrary.BlazorClient` do solution (pravý klik na solution → Add → Existing Project)
3. Nastavte Multiple Startup Projects:
   - Pravý klik na solution → Properties
   - V Common Properties → Startup Project vyberte "Multiple startup projects"
   - Nastavte Action = Start pro oba projekty (ELibrary.WebApp a ELibrary.BlazorClient)
4. Klikněte na Start (F5)

## Konfigurace

### Změna URL API

Pokud API běží na jiném portu, upravte `appsettings.json` v Blazor projektu:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:VÁŠE_PORT"
  }
}
```

## Funkce aplikace

### Správa knih
- **Zobrazit všechny knihy** - Načte všechny knihy z databáze
- **Vyhledávání** - Vyhledávání podle názvu, autora nebo ISBN
- **Půjčit knihu** - Půjčení knihy zákazníkovi (volitelné jméno zákazníka)
- **Vrátit knihu** - Vrácení knihy zpět do knihovny

### Historie zápůjček
- **Načíst historii** - Zobrazí všechny záznamy o půjčkách a vrácení knih
- Záznamy jsou seřazeny podle data sestupně (nejnovější nahoře)
- Zobrazuje název knihy, jméno zákazníka, akci (Borrowed/Returned) a datum

## Struktura projektu

```
ELibrary.BlazorClient/
├── Services/
│   └── BookApiService.cs             # HTTP klient pro komunikaci s API
├── Pages/
│   ├── Books.razor                   # Hlavní stránka aplikace
│   ├── _Host.cshtml                  # Entry point
│   └── _Layout.cshtml                # HTML layout
├── Shared/
│   └── MainLayout.razor              # Hlavní layout komponenty
├── wwwroot/
│   └── css/
│       └── site.css                  # Vlastní styly
├── _Imports.razor                    # Globální using direktivy
├── App.razor                         # Root komponenta
├── Routes.razor                      # Routing konfigurace
├── Program.cs                        # Startup konfigurace
└── appsettings.json                  # Konfigurace aplikace
```

## Architektura

- **Čistý UI klient** - Žádná business logika, pouze prezentační vrstva
- **BookApiService** - HTTP klient pro volání REST API endpointů
- **Používá sdílené DTOs** - Reference na `ELibrary.Shared` projekt pro DTOs
- **Blazor Server** - Server-side rendering s SignalR komunikací

## Poznámky

- Aplikace používá Bootstrap 5.3 pro styling
- Bootstrap Icons pro ikony
- Blazor Server pro real-time komunikaci
- HttpClient pro komunikaci s API
- Používá vaše existující DTOs z `ELibrary.Shared` projektu
