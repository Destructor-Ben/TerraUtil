# TerraUtil.BuildSystem

The better build system for tModLoader, the mod loader for Terraria.

Based on [this pull request](https://github.com/tModLoader/tModLoader/pull/2472) by Chik3r, with modifications made by me.

## Features

- `build.txt` properties moved to the `.csproj`.
- All references (`<Reference>`, `<ProjectReference>`, `<PackageReference>`, and `<ModReference>`) are all done inside the `.csproj` and are automatically added to the mod file.

## Future Features

- Allow mod code to be outside of `ModSources`
- Automatic workshop icon generation
- Automatic workshop description and changelog generation and compilation from markdown
- Effect compilation

# Licensing

This code is licensed under the MIT license.

Some code from tModLoader is used, which is licensed under the MIT license.

The used code is:

- Everything under the `Core` folder
- `Tasks/BaseTask.cs`
- `Tasks/PackageModFile.cs`
