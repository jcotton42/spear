using Microsoft.EntityFrameworkCore;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Spear.Models;
using Spear.Results;

namespace Spear.Services;

public class PromptService {
    private readonly AuthorizationService _authorization;
    private readonly ICommandContext _commandContext;
    private readonly SpearContext _spearContext;

    public PromptService(AuthorizationService authorization, ICommandContext commandContext, SpearContext spearContext) {
        _authorization = authorization;
        _commandContext = commandContext;
        _spearContext = spearContext;
    }

    public async Task<Result<int>> AddPromptAsync(string suggestion, CancellationToken ct) {
        if(!await _authorization.InvokerCanSubmitPromptsAsync(ct)) {
            return new SpearPermissionDeniedError(
                "You do not have permission to submit prompts",
                Permission.SubmitPrompts
            );
        }

        var prompt = new Prompt {
            GuildId = _commandContext.GuildID.Value,
            Submitter = _commandContext.User.ID,
            Text = suggestion,
        };

        _spearContext.Prompts.Add(prompt);
        await _spearContext.SaveChangesAsync(ct);

        return prompt.Id;
    }

    public async Task<Result> EditPromptAsync(int id, string newSuggestion, CancellationToken ct) {
        var prompt = await _spearContext.Prompts
            .FirstOrDefaultAsync(p => p.Id == id && p.GuildId == _commandContext.GuildID.Value, ct);
        if(prompt is null) {
            return new NotFoundError($"No prompt found with ID {id}");
        }

        if(!await _authorization.InvokerCanEditOrDeletePromptsAsync(prompt, ct)) {
            return new SpearPermissionDeniedError("You cannot edit this prompt", Permission.ModeratePrompts);
        }

        prompt.Text = newSuggestion;
        await _spearContext.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public async Task<Result> DeletePromptAsync(int id, CancellationToken ct) {
        var prompt = await _spearContext.Prompts
            .FirstOrDefaultAsync(p => p.Id == id && p.GuildId == _commandContext.GuildID.Value, ct);
        if(prompt is null) {
            return new NotFoundError($"No prompt found with ID {id}");
        }

        if(!await _authorization.InvokerCanEditOrDeletePromptsAsync(prompt, ct)) {
            return new SpearPermissionDeniedError("You cannot delete this prompt", Permission.ModeratePrompts);
        }

        _spearContext.Prompts.Remove(prompt);
        await _spearContext.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public async Task<Result<Prompt>> GetRandomPromptAsync(CancellationToken ct) {
        // TODO cache this by guild
        var ids = await _spearContext.Prompts
            .Where(p => p.GuildId == _commandContext.GuildID.Value)
            .Select(p => p.Id)
            .ToListAsync(ct);
        if(!ids.Any()) {
            return new NotFoundError("No prompts are available for this guild.");
        }

        var id = ids[Random.Shared.Next(ids.Count)];
        return await _spearContext.Prompts
            .AsNoTracking()
            .SingleAsync(p => p.Id == id, ct);
    }
}
