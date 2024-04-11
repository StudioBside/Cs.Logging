﻿namespace WikiTool.Core;

using Html2Markdown.Replacement;

public sealed class MyPatternComposite : CompositeReplacer
{
    public MyPatternComposite()
    {
        // remove '<a href="#개요" class="toc-anchor">¶</a>'
        this.AddReplacer(new PatternReplacer
        {
            Pattern = @"<a href=""#.*?"" class=""toc-anchor"">¶</a>",
            Replacement = string.Empty,
        });
    }
}
