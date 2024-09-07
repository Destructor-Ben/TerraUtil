# To Do List
- Icons
  - 128x128 for TerraUtil.API (NuGet)
  - 128x128 for TerraUtil.BuildSystem (NuGet)
  - 80x80 and either 512x512 or 480x480 for TerraUtil.DevTools (tML Mod)
  - Svg for Rider IDE
- Clean up everything below

## TerraUtil.BuildSystem
- Go through all code
- Make mods automatically symlink to ModSources
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

# OLD
# To Do List
- Fix how references are added - only add ones that are in the output folder? maybe keep a list of ignored assemblies?
  - Need to fix how terrautil.buildsystem is included in the modfile
- Why does AssemblyName need to be specified?
  - Ozzatron — 06/18/2024 11:03 AM
    Is it required for the ModSources/MyModName folder to be identically named to the mod's internal name?
    Or can I rename that folder at will without causing chaos or destruction?
    jopojelly — 06/18/2024 11:05 AM
    I believe you can't get around that easily.
    Ozzatron — 06/18/2024 11:23 AM
    Okay. No problem. Technically, I can rename the github repository without causing chaos, but blind clone operations will result in problems (i.e. not specifying the output folder)
    Destructor_Ben — 06/18/2024 10:07 PM
    iirc internal mod name is only determined by the folder name. namespaces, main mod file, csproj file, and mod class can be called whatever
    direwolf420 — 06/18/2024 10:09 PM
    except when you build in VS, then assemblyname in csproj also has to match (tho not sure if still applies in net8)
  - This folder thing is a bit annoying for git repos when users try downloading source code which always appends a version/branch string
  - The guide of primz (XD87) — 06/19/2024 2:20 AM
  you can build without a .csproj or code files (if you provide a dll), the namespace and mod class are used for validation during loading
  p sure its just the folder name
