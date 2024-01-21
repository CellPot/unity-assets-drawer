# Unity Assets Drawer

## Introduction

Assets placement in large quantities has never been easier!

This is a customizable Assets Drawer that allows you to create prefabs and erase assets from the scene using brush with radius zone. 
In many ways it works like Terrain Brush, but doesn't limit you to that type of surface and trees with other foliage

## Quick installation guide
1) Install the tool:
	- a: Download and import package file from releases into your project
	- b: ...OR Copy and paste `AssetsDrawer` directory into your project
2) Open ```Tools/Assets Drawer``` window
3) Fill prefabs palette:
	- a: Just open directory with your prefabs in Project Window and focus on the Assets Drawer window
	- b: ...OR Copy and paste path of that directory into `Persistent assets path` field
4) Set the surface layers, so you won't draw on everything
5) Set your assets layers or tag so overlapping and erasing logics can work correctly
6) All other properties and drawer settings are self-explanatory thanks to tooltips
7) Experiment a bit!

## Features overview
#### This tool has many useful features for different use cases and does not limit you to terrain type of surface:

#### 1) For example, you can use it to create trees and transform them into woods, while simultaneously changing and randomizing the properties of the objects being created
<img src="https://s13.gifyu.com/images/S0aC1.gif" alt="woods draw 1" border="0">

#### 2) ...And woods can be of different densities and compositions
<img src="https://s13.gifyu.com/images/S0aCB.gif" alt="woods draw 2" border="0">

#### 3) You can also set spacing and randomize created assets
<img src="https://s13.gifyu.com/images/S0aCA.gif" alt="spread spawn" border="0">

#### 4) All of that while assets can be aligned to surface normal vector
<img src="https://s13.gifyu.com/images/S0aaP.gif" alt="aligned spawn lr1" border="0" width="1200" height="550">

#### 5) And objects can be easily erased from the scene (if they are not nested into prefabs)
<img src="https://s13.gifyu.com/images/S0aCo.gif" alt="deletion" border="0">

#### 6) Also, already existing objects can be projected onto the surface and aligned to the normal vector at their position too
<img src="https://s13.gifyu.com/images/S0aYc.gif" alt="projection lr" border="0" />

