﻿using System;
using Cs.Repl;
using WikiTool.Cli;

var console = new ReplConsole();
if (await console.InitializeAsync(new WikiToolHandler()) == false)
{
    return;
}

await console.Run();