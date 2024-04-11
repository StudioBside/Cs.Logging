﻿namespace WikiTool.Cli;

using System.Text;
using System.Threading.Tasks;
using Cs.Repl;
using WikiTool.Core;

public sealed class WikiToolHandler : ReplHandlerBase
{
    private readonly WikiToolCore tool;

    public WikiToolHandler()
    {
        this.tool = new WikiToolCore();
    }
    
    public WikiToolConfig Config => this.tool.Config;
    
    public override Task<bool> InitializeAsync()
    {
        return this.tool.InitializeAsync();
    }
    
    public override string GetPrompt()
    {
        return this.tool.CurrentSpace?.Name ?? "No space";
    }
    
    [ReplCommand(Name = "spaces", Description = "컨플루언스 스페이스 목록을 출력합니다.")]
    public string GetSpaces(string argument)
    {
        var sb = new StringBuilder();
        foreach (var space in this.tool.Spaces.OrderBy(e => e.Name))
        {
            sb.AppendLine($"id:{space.Id} key:{space.Key} name:{space.Name}");
        }

        return sb.ToString();
    }

    [ReplCommand(Name = "set-space", Description = "입력받은 spaceId에 해당하는 스페이스를 선택합니다.")]
    public async Task<string> SetSpace(string argument)
    {
        if (int.TryParse(argument, out int spaceId) == false)
        {
            return $"Invalid space id: {argument}";
        }

        if (await this.tool.SetSpaceById(spaceId) == false)
        {
            return $"Space not found: {argument}";
        }
        
        return string.Empty;
    }

    [ReplCommand(Name = "view-space", Description = "현재 선택된 스페이스의 페이지 목록을 출력합니다.")]
    public string ViewSpace(string argument)
    {
        if (this.tool.CurrentSpace is null)
        {
            return "선택된 space가 없습니다.";
        }
        
        return this.tool.CurrentSpace.ToString();
    }

    [ReplCommand(Name = "convert-pages", Description = "입력받은 개수 만큼의 페이지 변환을 수행합니다.")]
    public Task<string> ConvertPages(string argument)
    {
        if (int.TryParse(argument, out int convertCount) == false)
        {
            return Task.FromResult($"변환할 페이지의 개수를 입력해야 합니다: {argument}");
        }
        
        return this.tool.ConvertPages(convertCount);
    }

    [ReplCommand(Name = "clean-garbages", Description = "제목에 id값 명시하지 않은 페이지가 있다면 일괄 삭제합니다.")]
    public Task<string> Deprecated_CleanGarbages(string argument)
    {
        return this.tool.CleanGarbages();
    }

    [ReplCommand(Name = "search-page", Description = "검색어 키워드를 입력받아 페이지를 검색합니다.")]
    public string SearchPages(string argument)
    {
        return this.tool.SearchPage(argument);
    }

    [ReplCommand(Name = "view-page", Description = "입력받은 (cfPage)id에 해당하는 페이지 내용을 api로 다시 받아옵니다.")]
    public Task<string> ViewPage(string argument)
    {
        return this.tool.ViewPage(int.Parse(argument));
    }

    [ReplCommand(Name = "convert-by-id", Description = "입력받은 (wjPage)id에 해당하는 페이지를 변환합니다.")]
    public Task<string> ConvertById(string argument)
    {
        var tokens = argument.Split(' ');
        int pageId = int.Parse(tokens[0]);
        bool force = tokens.Length > 1; // --force
        return this.tool.ConvertById(pageId, force);
    }
}
