using System.Collections.Generic;

namespace VArchiveHelper;

internal sealed class UsageGuideSection
{
	public string Title { get; init; }

	public IReadOnlyList<string> Lines { get; init; }
}
