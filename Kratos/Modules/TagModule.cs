﻿using System;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Discord.Commands;
using Kratos.Services;
using Kratos.Preconditions;
using Kratos.Data;

namespace Kratos.Modules
{
    [Name("Tag Module"), Group("tag")]
    public class TagModule : ModuleBase
    {
        private TagService _service;

        [Command]
        [Summary("Returns the value associated with the given tag. When not given an argument, returns all tag names.")]
        [RequireCustomPermission("tag.view")]
        public async Task Tag([Summary("The tag's value")] string tag = null)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            if (tag == null)
            {
                var tags = (await _service.GetTagsAsync()).Select(x => x.Tag);
                if (tags.Count() < 1)
                {
                    await dmChannel.SendMessageAsync(":x: No tags found.");
                    return;
                }
                var response = string.Join(", ", tags);
                await dmChannel.SendMessageAsync(response);
                _service.DisposeContext();
                return;
            }
            var entity = await _service.GetTagAsync(tag, true);
            if (entity == null)
            {
                await dmChannel.SendMessageAsync(":x: Tag not found.");
                return;
            }
            await dmChannel.SendMessageAsync($"{entity.Value}");
            _service.DisposeContext();
        }

        [Command("add"), Alias("+")]
        [Summary("Adds a tag.")]
        [RequireCustomPermission("tag.manage")]
        public async Task Add([Summary("The tag's key")] string key,
                              [Summary("The tag's value")] string value)
        {
            var result = await _service.TryAddTagAsync(new TagValue
            {
                Tag = key,
                Value = value,
                CreatedAt = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                CreatedBy = Context.User.Id
            });

            if (!result)
            {
                await ReplyAsync(":x: Tag already exists.");
                return;
            }
            await ReplyAsync(":ok:");
            _service.DisposeContext();
        }

        [Command("remove"), Alias("-")]
        [Summary("Removes a tag.")]
        [RequireCustomPermission("tag.manage")]
        public async Task Remove([Summary("The tag to remove")] string key)
        {
            var result = await _service.TryRemoveTagAsync(key);
            if (!result)
            {
                await ReplyAsync(":warning: Tag not found.");
                return;
            }
            await ReplyAsync(":ok:");
            _service.DisposeContext();
        }

        [Command("info"), Alias("?")]
        [Summary("Get metadata for a tag.")]
        [RequireCustomPermission("tag.info")]
        public async Task Info([Summary("The tag for which to get metadata")] string key)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            var entity = await _service.GetTagAsync(key, false);
            if (entity == null)
            {
                await dmChannel.SendMessageAsync(":x: Tag not found.");
                return;
            }
            var response = new StringBuilder();
            response.AppendLine($"{key}:");
            response.AppendLine($"Created at: {new DateTime(1970, 1, 1).AddSeconds(entity.CreatedAt)} UTC");
            var author = await Context.Guild.GetUserAsync(entity.CreatedBy);
            response.AppendLine($"Created by: {author.Username}#{author.Discriminator}");
            response.AppendLine($"Times invoked: {entity.TimesInvoked}");
            await dmChannel.SendMessageAsync(response.ToString());
            _service.DisposeContext();
        }

        public TagModule(TagService s)
        {
            _service = s;
        }
    }
}
