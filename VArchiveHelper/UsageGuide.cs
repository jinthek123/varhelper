using System.Collections.Generic;

namespace VArchiveHelper;

internal static class UsageGuide
{
	public const int ContentWidth = 600;

	public static IReadOnlyList<UsageGuideSection> Sections { get; } = new UsageGuideSection[6]
	{
		new UsageGuideSection
		{
			Title = "기능",
			Lines = new string[3] { "단축키로 지정 모니터 전체를 캡처합니다. (기본: DXGI)", "클립보드에 넣은 뒤 V-Archive 인식(Alt+Insert)을 실행합니다.", "V-Archive는 별도 설치가 필요합니다." }
		},
		new UsageGuideSection
		{
			Title = "처음 설정",
			Lines = new string[4] { "1. v-archive.exe 경로를 지정합니다.", "2. 출력 해상도와 캡처 모니터(Index 1, 2 …)를 맞춥니다.", "3. 캡처 단축키를 확인하거나 변경합니다.", "4. 「적용 및 저장」을 누릅니다." }
		},
		new UsageGuideSection
		{
			Title = "사용 순서",
			Lines = new string[3] { "1. 게임 또는 대상 화면을 실행합니다.", "2. 지정한 모니터에서 캡처 단축키를 누릅니다.", "3. 맨 위 「상태」 줄에서 완료 여부를 확인합니다." }
		},
		new UsageGuideSection
		{
			Title = "단축키",
			Lines = new string[4] { "· 「단축키 지정」 후 키를 누르면 즉시 저장됩니다.", "· Esc 키로 지정을 취소할 수 있습니다.", "· 캡처 단축키와 V-Archive 인식(Alt+Insert)은 별개입니다.", "· 「캡처 동작 실행」으로 단축키 없이 동작을 확인할 수 있습니다." }
		},
		new UsageGuideSection
		{
			Title = "옵션",
			Lines = new string[3] { "· DXGI 캡처 — 전체화면 게임에 권장합니다.", "· DXGI 필수 — DXGI 실패 시 캡처를 중단합니다.", "· 물리 픽셀 — 고배율·다중 모니터 보정에 사용합니다." }
		},
		new UsageGuideSection
		{
			Title = "참고",
			Lines = new string[3] { "· 단축키 외 변경 사항은 「적용 및 저장」이 필요합니다.", "· 창 제목의 * 는 저장하지 않은 변경이 있음을 뜻합니다.", "· 프로그램은 하나만 실행되며, 설정 창을 닫으면 종료됩니다." }
		}
	};
}
