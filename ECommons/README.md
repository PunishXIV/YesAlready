<section id="about">
<a href="#about" alt="About"><h1>About ECommons</h1></a>
  <p>ECommons is a multi-functional library designed to work within Dalamud Plugins. It features a variety of different systems and shortcuts which cuts out a lot of boiler plate code normally used to do standard plugin tasks.</p>
</section>

<section id="getting-started">
<a href="#getting-started" alt="Getting Started"><h2>Getting Started</h2></a>
Add ECommons as a submodule to your project:

```
git submodule add https://github.com/NightmareXIV/ECommons.git
```
Add it to your plugin's CSProj file:

```  
<ItemGroup>
    <ProjectReference Include="..\ECommons\ECommons\ECommons.csproj" />
</ItemGroup>
```

Then, in the entry point of your plugin:

```
ECommonsMain.Init(pluginInterface, this);
```

where pluginInterface is a <b>DalamudPluginInterface</b>.
</section>

<section id="using-modules">
<a href="#using-modules" alt="Using Modules"><h2>Using Modules</h3></a>
ECommons comes with various modules which needs to be initalised at plugin runtime. To do so, modify your initalising code as follows:

```
ECommonsMain.Init(pluginInterface, this, Modules.<Module>);
```

where \<Module> is one of the following:
- All (For all modules)
- Localization
- SplatoonAPI
- DalamudReflector
- ObjectLife
- ObjectFunctions
</section>
