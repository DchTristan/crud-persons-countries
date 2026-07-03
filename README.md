# CRUD Solution — Persons & Countries Management

Application web ASP.NET Core MVC de gestion de personnes et de pays, construite autour d'une architecture en couches inspirée de la **Clean Architecture**, avec injection de dépendances, pattern **Repository**, et une suite de tests unitaires/intégration.

L'application permet de créer, lister, rechercher, trier, modifier et supprimer des fiches "Personne" (nom, email, date de naissance, genre, pays, adresse, abonnement newsletter, numéro d'identification fiscale), de gérer une liste de pays (avec import en masse depuis un fichier Excel), et d'exporter la liste des personnes en **PDF**, **CSV** ou **Excel**.

## Sommaire

- [Fonctionnalités](#fonctionnalités)
- [Architecture](#architecture)
- [Stack technique](#stack-technique)
- [Tests](#tests)
- [Prérequis](#prérequis)
- [Installation et exécution](#installation-et-exécution)
- [Structure du projet](#structure-du-projet)
- [Limites connues / pistes d'amélioration](#limites-connues--pistes-damélioration)
- [Licence](#licence)

## Fonctionnalités

- **Gestion des personnes** : création, édition, suppression, recherche (par nom, email, date de naissance, genre, pays, adresse) et tri dynamique sur toutes les colonnes.
- **Gestion des pays** : ajout unitaire, et **import en masse depuis un fichier Excel** (EPPlus).
- **Export de la liste des personnes** :
  - **PDF** via [Rotativa](https://github.com/webgio/Rotativa.AspNetCore) (moteur `wkhtmltopdf`)
  - **CSV** via [CsvHelper](https://joshclose.github.io/CsvHelper/)
  - **Excel (.xlsx)** via [EPPlus](https://epplussoftware.com/)
- Validation des données via `DataAnnotations` côté DTO (`PersonAddRequest`, `PersonUpdateRequest`, `CountryAddRequest`).
- Age calculé dynamiquement à partir de la date de naissance (pas de colonne stockée).

## Architecture

Le projet est découpé en 7 projets .NET distincts, organisés en couches concentriques façon Clean Architecture :

```
CRUDExample (présentation : MVC, Controllers, Views, DI, Program.cs)
      │
      ├──> Services (logique métier : PersonsService, CountriesService)
      │        │
      │        ├──> ServiceContracts (interfaces de service + DTOs + enums)
      │        └──> RepositoryContracts (interfaces d'accès aux données)
      │
      ├──> Repositories (implémentation EF Core des repositories)
      │        └──> Entities (modèles de domaine + DbContext + migrations)
      │
      └──> Entities
```

**Principe respecté :** les couches internes (`ServiceContracts`, `RepositoryContracts`) ne dépendent d'aucune couche externe — aucune inversion de dépendance n'est violée. `Services` ne connaît que les **interfaces** de repository (`IPersonsRepository`, `ICountriesRepository`), jamais l'implémentation EF Core concrète ni le `DbContext` directement : le remplacement de la base de données (ou son mock en test) ne nécessite aucun changement dans la couche métier.

**Écart par rapport à une Clean Architecture stricte :** le projet `Entities`, censé être le cœur pur du domaine, contient en réalité le `ApplicationDbContext` (EF Core), les migrations, et des appels directs à des procédures stockées SQL Server. Dans une Clean Architecture "manuel", ces éléments d'infrastructure seraient isolés dans un projet séparé (ex. `Infrastructure` ou `Persistence`), pour que `Entities` ne dépende d'aucun framework externe. Ici, c'est un compromis pragmatique assez courant dans les projets de taille modeste.

### Pattern Repository

Chaque type de donnée (Person, Country) est accédé via une interface dédiée (`IPersonsRepository`, `ICountriesRepository`) implémentée par une classe concrète (`PersonsRepository`, `CountriesRepository`) qui encapsule les requêtes EF Core. Les services métier ne manipulent que ces interfaces, injectées via le conteneur DI natif d'ASP.NET Core (`AddScoped<...>` dans `Program.cs`).

Bénéfices concrets démontrés dans ce projet :
- Les tests unitaires de `PersonsService` mockent entièrement `IPersonsRepository` avec **Moq** — aucune base de données réelle n'est nécessaire pour tester la logique métier (calcul d'âge, tri, validation...).
- Les tests d'intégration remplacent le `DbContext` SQL Server par une base **EF Core InMemory** (`CustomWebApplicationFactory`), sans toucher au code de l'application.

## Stack technique

| Catégorie | Technologies |
|---|---|
| Framework | .NET 8 / ASP.NET Core MVC |
| ORM | Entity Framework Core 9 (SQL Server / LocalDB) |
| Export | Rotativa.AspNetCore (PDF), CsvHelper (CSV), EPPlus (Excel) |
| Tests | xUnit, Moq, AutoFixture, FluentAssertions, EntityFrameworkCore.InMemory, Microsoft.AspNetCore.Mvc.Testing, Fizzler + HtmlAgilityPack (parsing HTML en tests d'intégration) |

## Tests

Le projet `CRUDTests` couvre plusieurs niveaux :

- **Tests unitaires de service** (`PersonsServiceTest`) : logique métier de `PersonsService` (ajout, récupération, filtrage, tri, mise à jour, suppression) testée en isolation via un mock de `IPersonsRepository` (Moq) et des données générées avec AutoFixture.
- **Tests unitaires de service** (`CountriesServiceTest`) : validation des règles métier d'ajout/récupération de pays.
- **Tests unitaires de contrôleur** (`PersonsControllerTest`) : comportement de `PersonsController` (vues retournées, redirections) en mockant les services.
- **Test d'intégration bout-en-bout** (`PersonsControllerIntegrationTest`) : démarre l'application complète via `WebApplicationFactory` (base InMemory), effectue une vraie requête HTTP `GET /Persons/Index`, et vérifie le HTML rendu avec Fizzler/HtmlAgilityPack.

> **Note transparence :** `CountriesServiceTest` instancie actuellement `CountriesService` avec un repository `null` — le mock EF Core préparé dans le constructeur n'est donc pas réellement exercé. Voir la section [Limites connues](#limites-connues--pistes-damélioration).

## Prérequis

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (installé avec Visual Studio, ou via [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads))

## Installation et exécution

```bash
git clone https://github.com/<ton-user>/<ton-repo>.git
cd <ton-repo>

# Restaurer les dépendances NuGet
dotnet restore

# Appliquer les migrations EF Core (crée la base PersonsDatabase sur LocalDB)
dotnet ef database update --project Entities --startup-project CRUDExample

# Lancer l'application
cd CRUDExample
dotnet run
```

L'application est accessible sur **http://localhost:5039** (voir `Properties/launchSettings.json`).

Pour lancer la suite de tests :

```bash
dotnet test
```

## Structure du projet

```
CRUDSolution/
├── CRUDExample/           # Présentation : Controllers, Views, Program.cs, wwwroot (dont wkhtmltopdf)
├── CRUDTests/              # Tests unitaires et d'intégration
├── Entities/               # Modèles de domaine (Person, Country), DbContext, migrations EF Core
├── Repositories/           # Implémentations EF Core des repositories
├── RepositoryContracts/    # Interfaces de repository
├── ServiceContracts/       # Interfaces de service, DTOs, enums
└── Services/                # Logique métier, export CSV/Excel
```

## Limites connues / pistes d'amélioration

Cette section liste honnêtement les points identifiés lors d'une revue de code, à corriger dans une prochaine itération :

- **`PersonsRepository.UpdatePerson`** : la condition de vérification est inversée — la méthode retourne la personne sans persister la moindre modification lorsqu'une correspondance est trouvée en base.
- **`PersonsService.GetPersonsCSV`** : les lignes de données sont écrites deux fois dans le flux CSV généré (une fois via `CsvWriter.WriteRecordsAsync`, une fois via une boucle manuelle), produisant un fichier avec des doublons.
- **`CountriesController.UploadFromExcel`** : la vérification de l'extension du fichier compare `Path.GetExtension(...)` (qui retourne `".xlsx"`, avec un point) à `"xlsx"` (sans point), ce qui fait échouer la validation même pour un fichier `.xlsx` valide.
- **`CountriesServiceTest`** : le service testé est instancié avec un repository `null`, rendant inopérant le mock EF Core préparé en amont.
- **Aucune authentification/autorisation** : l'application est actuellement ouverte à tout utilisateur, sans notion de compte ni de rôle — à considérer avant tout déploiement public.
- Le projet `Entities` mélange modèle de domaine et infrastructure EF Core (voir [Architecture](#architecture)) ; une séparation en projet `Infrastructure` distinct rapprocherait le projet d'une Clean Architecture stricte.

## Licence

Distribué sous licence [MIT](LICENSE).
