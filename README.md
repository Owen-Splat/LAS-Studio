# LAS-Studio
The go-to tool for making romhacks of Link's Awakening Switch! Currently just a level editor

You can download it here: https://github.com/Owen-Splat/LAS-Studio/releases/latest

## Information
This currently includes basic object editing features. You can move objects, change objects, add/delete objects, etc. Things like changing level music, environment, room camera boundaries, etc. are NOT yet supported

## How to run:
In order to run this application, you must have the .NET 6.0 Desktop Runtime installed, either x64 or x84 depending on your system

x64: https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.36-windows-x64-installer

x86: https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.36-windows-x86-installer

## Libraries
- GLFrameworkEngine : A 3D engine used for manipulating 3D opengl data with gizmo tools, selection handling, ray casting, and much more. This also includes various helper classes to handle OpenGL easier.  
- Map Studio UI : The UI engine used to display this editor running off imgui. 
- Toolbox.Core : A small, portable backend used for file IO to read/write data along with commonly shared things like texture swizzling and decoding. 
