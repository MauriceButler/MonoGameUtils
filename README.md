# MonoGame Utils

Some script and helpers for creating, working with, and packing cross platform MonoGame projects.

Started life as a bunch of hacked together scripts to improve my workflow.

# Install

``` bash
dotnet tool install -g MonoGameUtils
```

# Usage:
mgu <command> [<args>]

## CREATE:

Create a new solution with a shared project and platform projects:

``` bash
mgu create <gameName> [--desktopgl (default)] [--android] [--ios]
```

## PACKAGE:

Package game for each platform:

``` bash
mgu package [--all (default)] [--windows] [--linux] [--mac] [--android] [--ios]
```