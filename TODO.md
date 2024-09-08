# To Do List
- Icons
  - 128x128 for TerraUtil.API (NuGet)
  - 128x128 for TerraUtil.BuildSystem (NuGet)
  - 80x80 and either 512x512 or 480x480 for TerraUtil.DevTools (tML Mod)
  - Svg for Rider IDE
- Clean up everything below

## TerraUtil.BuildSystem
- [Done] Go through all code
- [Done] Make mods automatically symlink to ModSources
- [Done] Output tmod files to bin as well
- [Done] Make mod file ignoring better
- Clean up references
- Clean up properties
- [Done] Unify internal names - Can only change if symlinks are used
  - Make an internal name property, and set assembly name to it. Default internal name to the project name. Use the internal name in symlinking too.
  - Ignore the below
    - Irrelevant names
      - Mod class
      - Csproj name
      - Folder name
    - Relevant names
      - Assembly name
      - Root namespace
      - Add a mod name property?
- Effect compiler
- Workshop icon generation
- Workshop description & changelog generation

## TerraUtil.API
- Fix and clean up

## TerraUtil.DevTools
- Change the build action to run dotnet build
- Shader + asset hot reloads
- Could look into what other dev mods do

## TerraUtil.TestMod
- Make a good test

## TerraUtil.GitHubActions
- Various GitHub actions
  - Lint
  - Format
  - Build
  - Publish
  - Post mod file to Discord web hook

## TerraUtil.TemplateMod
- Template mod for TerraUtil
