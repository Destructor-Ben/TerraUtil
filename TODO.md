# To Do List

- Port modSource in build.txt for symlinking
  - What about using a custom mod name?
- Check if any new things have been introduced to building in tML that need to be accounted for
- Replace all \ with /
- Check that a bunch of random assemblies aren't included in the built mod file
  - https://github.com/tModLoader/tModLoader/pull/2472/files#diff-4004d18fdc5377ec036d956317da9f818e034f5b954527e03fd095388c57a63a

- Icons
  - 128x128 for TerraUtil.API (NuGet)
  - 128x128 for TerraUtil.BuildSystem (NuGet)
  - 80x80 and either 512x512 or 480x480 for TerraUtil.DevTools (tML Mod)
  - Svg for Rider IDE
- Clean up everything below

## TerraUtil.BuildSystem
- Effect compiler
- Source generator for effects
- Source generator for assets - Create fields for them, similar to AssGen?
- Workshop icon generation
- Workshop description & changelog generation (from possible a .tmd [terramarkdown] file?)
- Allow mod properties to be set from a .toml file
- Assembly publicising

### Shader Compiling


  <!-- Compile shaders -->
  <!-- TODO: fxc has to be added to path to work, add to tML at some point and make it work for mac and linux users
  <Target Name="CompileShader" BeforeTargets="Build" Condition="$(TerraUtil-CompileShaders)">
    <!- Finding old effect files and effect source files
    <ItemGroup>
      <EffectFiles Include="Assets/Effects/**/*.fx" />
      <FxcFiles Include="Assets/Effects/**/*.fxc" />
    </ItemGroup>

    <!- Delete the old fxc files ->
    <Delete Files="%(FxcFiles.Identity)" Condition="Exists('%(FxcFiles.Identity)')" />
    <!- Compile the fx files ->
    <Exec Command="fxc /T fx_2_0 /Fo Assets/Effects/$([System.IO.Path]::GetFileNameWithoutExtension('%(EffectFiles.FullPath)')).fxc /Od %(EffectFiles.Identity)" Condition="Exists('%(EffectFiles.Identity)')" />
  </Target>
  -->

  <!-- Remove compiled shader and XNA files from editor -->
  <!-- TODO: move this to the build system
  <ItemGroup>
    <None Remove="**\*.xnb" />
    <None Remove="**\*.fxc" />
  </ItemGroup>-->


  <PropertyGroup>
    <!-- TODO: make this opt out -->
    <!-- TODO: move this to the build system
    <NoWarn>MSB3270;MSB3277;X4717;$(NoWarn)</NoWarn>-->
  </PropertyGroup>

### Cleaning Up
- Maybe make symlinking an opt in feature?
- Clean up references
  - The main issue is that references have to be inside the mod folder to be added
  - Also, ignoring references can be weird and inconsistent, with Private being one way, but I've also heard of others
  - Perhaps make a property (e.g. `Packed="true"`) that must be enabled if people want to include a dll in the mod?
- Maybe don't make the root namespace get set and leave it up to the modder?
  - Make an internal name property, and set assembly name to it. Default internal name to the project name. Use the internal name in symlinking too.
  - Ignore the below
    - Irrelevant names, just me figuring out what actually matters
      - Mod class
      - Csproj name
      - Folder name
    - Relevant names
      - Assembly name
      - Root namespace
      - Add a mod name property?

## TerraUtil.API
- Fix and clean up

## TerraUtil.DevTools
- Change the build action to run dotnet build
- Shader + asset hot reloads
- Could look into what other dev mods do

# Stuff I haven't started yet

## TerraUtil.GitHubActions
- Various GitHub actions
  - Lint
  - Format
  - Build
  - Publish
  - Post mod file to Discord web hook
  - Make GitHub release

## TerraUtil.TemplateMod
- Template mod for TerraUtil
