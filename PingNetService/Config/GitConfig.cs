namespace tobeh.TypoContentService.Server.Config;

public class GitConfig{
    public required string RepositoryUrl { get; init; }
    public required string AccessToken { get; init; }
    public required string Path { get; init; }
};