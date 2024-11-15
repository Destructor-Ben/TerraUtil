# TerraUtil.BuildSystem

The better build system for tModLoader, the mod loader for Terraria.

Based on [this pull request](https://github.com/tModLoader/tModLoader/pull/2472) by Chik3r, with modifications made by me.

## Features

- `build.txt` properties moved to the `.csproj`.
- `tModLoader.targets` is automatically imported.
- The output `.tmod` file is also located in the `bin` folder.
- Moved `buildIgnore` to a `.buildignore` file that uses the same syntax as `.gitignore`.
- All references - `<Reference>`, `<ProjectReference>`, `<PackageReference>`, and `<ModReference>` - are done inside the `.csproj` and are automatically added to the mod file.
    - Note that dll references must be located inside the mod's folder (e.g. under `ModSources/ModName/` and it's subfolders) in order to be packaged. This is due to restrictions with MSBuild and may be changed in the future.
- The mod's code can be located outside of `ModSources` and still function properly. This works by symlinking the mod into ModSources.
    - The IDE used to build the mod or `dotnet build` must be run with admin privileges when first creating the symlink, since it requires admin access. Subsequent builds do not require this.
    - This may be changed to opt-in sometime in the future.
- Allowed changing the internal name of a mod through the `.csproj` with `<InternalName>`.
    - This requires the mods code to be outside of `ModSources` because the name of the folder in `ModSources` is normally used to determine the internal name.
- The root namespace and assembly name of the mod are automatically set to the internal name.
    - This may be changed in the future, let me know if you have any issues with this.

## Example

See [TerraUtil.TestMod](https://github.com/Destructor-Ben/TerraUtil/tree/main/src/TerraUtil.TestMod) for an example of how to use this package.

## Licensing

This code is licensed under the MIT license.

Some modified code from tModLoader is used, which is licensed under the MIT license.

The used code is:

- Everything under the `Core` folder
- `Tasks/BaseTask.cs`
- `Tasks/PackageModFile.cs`
