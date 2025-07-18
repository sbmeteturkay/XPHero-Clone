 XPHero-Clone

 Readme created by ai. 

This is the main Unity project for **XPHero**, a prototype built on a modular architecture with reusable core systems managed through a Git submodule package.

## 🔧 Structure

- `Packages/com.mete.rapidprototype`: Core systems package (submodule)
  - Contains architecture, features, utilities, and base components.
- `Assets`: Game-specific assets and scenes.
- `ProjectSettings`: Unity project configuration.

## 📦 Dependencies

This project depends on the following external packages:

| Name            | Source                          | Purpose                          |
|-----------------|----------------------------------|----------------------------------|
| Extenject       | OpenUPM / GitHub                | Dependency Injection             |
| R3              | OpenUPM                         | Reactive State Machines          |
| PrimeTween      | Git URL / OpenUPM               | Tweening System                  |
| UniTask         | Git URL / OpenUPM               | Async/Await for Unity            |
| NuGet for Unity | [nugetforunity.com](https://www.nugetforunity.com) | Manage .NET dependencies in Unity |

These dependencies are declared either in `manifest.json` or via `.asmdef` references inside the `Packages/com.mete.rapidprototype` folder.

## 🧩 Core Features (from Submodule)

The `com.mete.rapidprototype` package provides:

- Feature-based modular architecture
- MVC-like separation
- DI via Extenject
- Tooling support for Unity Editor
- Runtime systems (input, camera, UI, etc.)

The package is managed via Git submodules to allow reuse across multiple projects.
