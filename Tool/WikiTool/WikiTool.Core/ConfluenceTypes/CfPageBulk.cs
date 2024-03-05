﻿namespace WikiTool.Core.ConfluenceTypes;

using Cs.Core.Util;
using Newtonsoft.Json.Linq;

public sealed class CfPageBulk
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public required string Status { get; set; }
    
    public static CfPageBulk LoadFromJson(JToken obj, int index)
    {
        return new CfPageBulk
        {
            Id = obj.GetInt32("id"),
            Title = obj.GetString("title"),
            Body = obj.GetString("body"),
            Status = obj.GetString("status"),
        };
    }
}

/*
        {
            "parentType": null,
            "createdAt": "2024-02-01T02:34:14.885Z",
            "authorId": "70121:837c953e-d704-419c-a5db-e1a80bb4134e",
            "id": "24281301",
            "version": {
                "number": 1,
                "message": "",
                "minorEdit": false,
                "authorId": "70121:837c953e-d704-419c-a5db-e1a80bb4134e",
                "createdAt": "2024-02-01T02:34:18.865Z"
            },
            "title": "스타 테크",
            "status": "current",
            "ownerId": "70121:837c953e-d704-419c-a5db-e1a80bb4134e",
            "body": {},
            "parentId": null,
            "spaceId": "24281093",
            "lastOwnerId": null,
            "position": 854,
            "_links": {
                "editui": "/pages/resumedraft.action?draftId=24281301",
                "webui": "/spaces/QkL2rjCmJyjr/overview",
                "tinyui": "/x/1YByAQ"
            }
        },
*/