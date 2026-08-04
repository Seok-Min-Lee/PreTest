# 🧩 구현 노트 (Implementation Notes)

> `Docs/Roadmap.md`에 있는 각 작업 항목의 상세 구현 배경·트레이드오프 기록. "무엇을 했는지"는 Roadmap, "왜 이렇게 했는지"는 여기.

## Priority 1: 코어 레이저 및 수신기 시스템

### 레이저 앞/뒷면 판별

볼록 콜라이더는 항상 진입면(레이저 방향과 반대인 면)만 히트되므로 `Vector3.Dot(direction, hit.normal)` 비교만으로는 앞/뒷면을 구분할 수 없음. 대신 `ILaserReflector.ReflectiveNormal`(반사체가 정의한 정면 벡터)과 `hit.normal`을 임계값(`FrontFaceDotThreshold`) 기반으로 비교하여, 반사체가 설계한 정면 방향과 일치할 때만 반사되도록 처리.

### 레이저 궤적 색상 피드백 — `LineRenderer.startColor`가 아닌 `_EmissionColor`

처음엔 `LineRenderer.startColor`/`endColor`(버텍스 컬러)로 판정 결과를 표시하려 했으나, 실제 게임뷰에는 반영되지 않는 문제가 있었음 — 원인은 Laser 머티리얼(`Assets/Materials/Laser/Laser.mat`)이 URP `Lit` 셰이더를 쓰는데, 이 셰이더가 버텍스 컬러를 아예 읽지 않고, 눈에 보이는 빛도 `_BaseColor`가 아니라 HDR `_EmissionColor`(블룸)가 만들어내고 있었기 때문. `MirrorGhost`(`Assets/Scripts/Placement/MirrorGhost.cs`)가 이미 쓰던 `MaterialPropertyBlock` 패턴을 그대로 가져와 `_EmissionColor`를 직접 덮어쓰는 방식으로 교체 — 머티리얼 인스턴스를 새로 만들지 않아 여러 `LaserEmitter`가 있어도 서로 영향 없음.

판정 결과는 `LaserResult`(`Default`/`Success`/`Failure`) 3단계로 표현. `SimulateLaser()`의 종료 지점(허공으로 빠짐 / 반사체가 아닌 대상에 도달)에서 `reflectionCount`가 `MaxReflectionCount`에 도달했는지로 `Failure`(더 이상 반사 기회가 없음)와 `Default`(아직 기회가 남음)를 구분하고, `ILaserHitReceiver`에 도달하면 `Success`. 세 색상(`_defaultColor`/`_successColor`/`_failureColor`) 모두 `[ColorUsage(true, true)]`로 Inspector에서 HDR 강도까지 조절 가능하게 노출.

## Priority 2: 거울 배치 및 조작 시스템

### 오브젝트 풀링과 ActiveCount

`PlacedMirror.ActiveCount`는 재사용 시 다시 실행되지 않는 `Awake`/`OnDestroy` 대신 `SetActive`마다 호출되는 `OnEnable`/`OnDisable`로 카운트하도록 변경 — 그래야 풀에서 꺼내 쓸 때도 활성 개수가 정확히 갱신됨.

### 자유 배치로 전환한 이유

최초 로드맵의 "Raycast + Surface Normal 정렬"에서 한 차례 `FloorGrid` 기반 바닥 그리드 클릭 배치(스타크래프트 건물 배치 방식)로 좁혀졌었으나, Inspector에서 Position/Rotation을 자유 편집하는 기능을 준비하며 그리드 점유 정보가 실제 위치와 어긋나는 문제를 피하기 위해 초기 배치 자체를 다시 전체 자유 배치로 통일함. `FloorGrid.cs`는 이 과정에서 삭제.

### 거울 개수 표시와 Add Mirror 버튼

좌측 하단 "현재 수 / 최대 수" 텍스트는 `MirrorCountDisplay`가 `PlacedMirror.ActiveCountChanged`(정적 이벤트, `OnEnable`/`OnDisable`에서 발생) 구독해서 갱신. 최대 개수는 `GameConfig.Instance.MaxMirrorCount`를 직접 참조(아래 "설정값을 한 곳에서 관리" 참고). 도달 시 `Add Mirror` 버튼을 `interactable = false`로 비활성화하는 처리도 한때 있었으나, `BeginPlacement`의 조기 반환으로 이미 기능적으로 막혀 있고 이 카운트 텍스트가 시각적 피드백을 대신하므로 제거함.

### 최대 개수 도달 피드백 — 이벤트로 디커플링

`MirrorPlacementController.BeginPlacement()`가 최대 개수 도달로 조기 반환할 때 `Debug.Log` 안내와 함께 `MirrorCountDisplay`의 개수 텍스트를 빨갛게 깜빡여 시각적으로도 알려줌. 처음엔 `MirrorPlacementController`가 `MirrorCountDisplay`를 직접 참조해 `FlashMaxReached()`를 호출했으나, `MirrorSelectionController.SelectionChanged`/`GameManager.ModeChanged` 등 이 프로젝트 전반의 관례(발행 쪽은 이벤트만 노출하고, 구독 쪽이 알아서 반응)와 맞지 않아 `MirrorPlacementController.MaxMirrorCountReached` 이벤트로 교체 — `MirrorCountDisplay`가 `OnEnable`/`OnDisable`에서 구독·해제하고 색상 트윈은 스스로 처리. 이 프로젝트에 설치된 DOTween엔 TextMeshPro 전용 모듈이 없어 `.DOColor()` 같은 확장 메서드를 못 쓰므로, `DOTween.To(() => _countText.color, ...)` 형태의 범용 값 트윈으로 `_countText.color`를 직접 조작. `SetLoops(4, LoopType.Yoyo)`로 원래색→빨강→원래색→빨강→원래색(2회 깜빡) 후 정확히 원래 색으로 종료되고, 연타 시 기존 트윈을 `Kill()`하고 색을 리셋한 뒤 새로 시작해 겹치지 않게 함.

### Gizmo 축별 핸들 — Move/Rotate 계산 방식

Move는 "카메라 방향과 선택된 축을 함께 포함하는 평면"에 `Plane.Raycast`로 교차시킨 뒤 그 교차점을 축 위로 투영해 그랩 지점 기준 델타만큼 이동시키는 방식(점프 없음). Rotate는 그 축을 법선으로 한 평면 위에서 시작 방향과 현재 방향의 `Vector3.SignedAngle`을 구해 회전시키는 방식 — 예전에는 이 축이 표면 법선 하나로 고정돼 있었는데, 이제는 선택한 핸들의 축으로 파라미터화됨.

### Gizmo Rotate 핸들 메시/콜라이더

Rotate 핸들은 회전축을 직관적으로 보여주기 위해 구체 대신 `Assets/Models/ring.fbx` 토러스 메시로 교체(재질은 기존 축 색상 그대로 유지). 유니티 임포터가 안정적 해시 기반 `fileID`(`fileIdsGeneration: 2`)를 쓰는 탓에 씬 텍스트를 직접 편집해 메시를 연결할 수 없어, `Assets/Editor/GizmoRotateHandleSetup.cs`(메뉴: Tools > Gizmo > Rotate 핸들에 Ring 메시 적용)로 에디터에서 정확히 연결. 판정용 `Collider`도 `SphereCollider`에서 `MeshCollider`(non-convex)로 교체 — convex를 켜면 링 가운데 빈 구멍이 볼록 껍질로 메워져 클릭 판정이 부정확해지므로, 레이캐스트 전용 정적 콜라이더 특성을 살려 non-convex로 실제 링 형태를 그대로 유지.

### Gizmo Move/Rotate 모드 분리

Move 핸들(원점 기준 오프셋 배치)과 Rotate 핸들(원점을 감싸는 링)이 동시에 노출되면 위치가 겹쳐 레이캐스트 판정이 꼬일 수 있어, 유니티 에디터의 `W`(Move)/`E`(Rotate) 단축키처럼 `MirrorGizmo._mode` 하나로 두 세트 중 한쪽만 활성화하도록 분리. 드래그 중(`_draggingHandle != null`)에는 입력을 무시해 조작 도중 모드가 바뀌지 않도록 함.

### Edit Option 버튼과 Gizmo 모드 동기화

좌측 상단 `Edit Option`의 `Position`/`Rotation` 버튼도 이 모드를 양방향으로 조작·표시하도록 `GizmoModeToggle`로 연동. `MirrorGizmo.SetMode`를 `public`으로 열고 모드가 바뀔 때 `ModeChanged` 이벤트를 쏘도록 해, 키보드(W/E)든 버튼 클릭이든 같은 진입점을 거치게 함. 각 버튼 위에 미리 준비된 `cover` 이미지(버튼 전체를 덮는 하이라이트, 클릭은 `Selectable`이 부모 계층까지 훑어 여전히 버튼에서 받음)를 현재 모드에 해당하는 쪽만 `SetActive(true)`로 켜서 시각적으로 동기화.

## Priority 3: UI 구조 및 양방향 동기화

### GameManager 모드 게이팅 — 배치

`MirrorPlacementController`도 `ModeChanged`를 구독(`OnEnable`/`OnDisable` — 이 컴포넌트는 자기 자신을 끄지 않으므로 `EditModeOnly`와 달리 `Awake`/`OnDestroy`가 아닌 `OnEnable`/`OnDisable`로 충분)해 두 겹으로 게이팅함: `BeginPlacement()` 진입 시점에 `Mode == Edit`가 아니면 조기 반환(버튼이 `EditModeOnly`로 숨겨져 있어 평소엔 호출 자체가 안 되지만 방어적으로 체크), 그리고 배치 도중(`_isPlacing == true`)에 Play로 전환되면 `HandleModeChanged`가 `CancelPlacement()`를 호출해 고스트를 즉시 숨기고 배치를 중단시킴.

### ModeIndicator 분리

좌측 상단 모드 표시(배경 Image + 아이콘 + TMP 텍스트)는 `GameManager`가 직접 만지지 않고 별도 `ModeIndicator` 클래스로 분리 — `GameManager.SetMode`가 `ModeIndicator.SetMode(mode)`를 호출하면, 배경·아이콘·텍스트 색과 텍스트 내용을 한 번에 갱신(Edit=시안색, Play=`Play Button`과 동일한 초록).

### ModeButtonGroup

`Edit Button`/`Play Button` 활성화 상태도 `GameManager`가 직접 건드리지 않고 `ModeButtonGroup`이 `GameManager.ModeChanged` 이벤트를 구독해 처리 — 현재 모드에 해당하는 버튼은 `SetActive(false)`로 숨기고 반대쪽만 보이게 함(두 버튼이 같은 자리에 겹쳐 있었기 때문). 기존에 나뉘어 있던 두 버튼 GameObject는 그대로 두고, `ModeButtonGroup` 전용 부모 오브젝트로 감싸서 이 스크립트가 그 부모에 붙도록 구성.

### EditModeOnly

`Add Mirror`/`Delete`/`Edit Option`(Position·Rotation 토글) 버튼처럼 Play 모드에서 아예 안 보여야 하는 UI는 매번 새 클래스를 만들지 않고, 범용 `EditModeOnly` 컴포넌트를 해당 오브젝트에 붙이는 방식으로 통일. `Edit` 모드가 아니면 자기 자신을 `SetActive(false)`. 단, 자기 자신을 끄는 컴포넌트라 `OnEnable`/`OnDisable`로 이벤트를 구독하면 한 번 꺼진 뒤 다시 켜질 신호를 받을 수 없어서, `Awake`/`OnDestroy`로 구독(순수 C# event는 비활성 오브젝트에도 정상 호출됨)해 이 문제를 피함.

### 선택 상태와 편집 가능 상태의 분리

정책상 Play 모드에서도 거울 **선택과 Inspector 값 표시는 계속 되어야 하지만**, Delete·InputField 편집·Move/Rotate 기즈모 조작은 막혀야 함 — 이 때문에 "선택 상태"와 "기즈모로 편집 가능한 상태"를 분리하는 리팩터링이 필요했음. `MirrorSelectionController`가 더 이상 `MirrorGizmo`를 직접 참조해 `Attach`/`Detach`를 호출하지 않고, 대신 `SelectedMirror` 프로퍼티와 `SelectionChanged` 이벤트만 노출. `MirrorGizmo`와 `MirrorInspectorController`가 각자 이 이벤트를 구독해서 독립적으로 반응(Gizmo는 Edit 모드일 때만 `SetActive(true)`, Inspector는 모드와 무관하게 항상 값 표시).

### MirrorGizmo 초기 비활성 문제

`MirrorGizmo`는 씬에서 기본 비활성(`m_IsActive: 0`)으로 시작했는데, 이 상태로는 `Awake`가 아예 실행되지 않아 `SelectionChanged`/`ModeChanged` 구독 자체가 안 걸리는 문제가 있었음(비활성 오브젝트는 처음 활성화되기 전까지 `Awake`가 호출되지 않음). `EditModeOnly`와 같은 이유로, 씬에서 기본 활성 상태로 바꾸고 `Awake` 시점에 구독 후 스스로 `UpdateVisibility()`로 즉시 숨기는 방식으로 변경.

### Name/Description 필드

`Name`/`Description` InputField도 Position/Rotation과 동일한 패턴으로 우선 연동함(데이터 영속 기능은 Priority 4 예정이라 값 자체는 `PlacedMirror.DisplayName`/`Description`이라는 단순 문자열 프로퍼티에만 보관). Position/Rotation과 달리 Gizmo 같은 외부 변경 주체가 없어 `Update`의 매 프레임 변경 감지 대상(`RefreshFieldsIfChanged`)에는 넣지 않고, 선택 변경 시 `ForceRefreshFields`에서만 동기화. 오브젝트 풀링 재사용 시 이전 배치의 값이 남지 않도록 `PlacedMirror.OnEnable`에서 매번 빈 문자열로 초기화.

### Inspector 패널 Close/재오픈

Inspector 하단 `Close Button`으로 패널을 닫고 거울을 클릭하면 다시 열리도록 `InspectorPanelToggle`을 추가. 패널 GameObject(`Inspector`)에 직접 붙이면 꺼진 동안 이 컴포넌트도 같이 비활성화되어 다시 열어줄 이벤트조차 못 받으므로, 항상 켜져 있는 부모 `Canvas`(Sub-Canvas 루트)에 붙여 `OnEnable`/`OnDisable`로 안전하게 구독. 이미 선택된 거울을 다시 클릭한 경우 `MirrorSelectionController.SelectionChanged`는 (선택 대상이 안 바뀌었으므로) 발생하지 않아 재오픈 신호로 못 쓰기 때문에, "클릭"과 "선택 변경"을 분리한 별도의 `MirrorClicked` 이벤트를 추가해 여기에 연결. Close 버튼의 `OnClick`도 다른 버튼들과 같은 방침대로 코드 `AddListener` 없이 `public OnClickClose()`를 씬에서 직접 연결.

### 선택된 거울 없을 때 Inspector InputField 잠금

Edit 모드라도 선택된 거울이 없으면 Inspector InputField를 편집 불가로 막아야 함 — 막지 않으면 빈 필드에 값을 입력하고 `OnEndEdit`가 발생했을 때 `ApplyPosition`/`ApplyRotation`이 `_target == null`인 채로 `ForceRefreshFields()`를 호출해 `_target.transform`에서 NullReferenceException이 나는 실제 버그가 있었음(이전엔 `interactable`이 모드에만 의존해서, 선택 없이도 Edit 모드면 필드가 활성 상태였음). `MirrorInspectorController.UpdateInteractable()`을 모드뿐 아니라 `_target != null`도 함께 확인하도록 바꾸고, 모드 변경 시점(`HandleModeChanged`)과 선택 변경 시점(`HandleSelectionChanged`) 양쪽에서 호출하도록 통일. 방어적으로 `ApplyPosition`/`ApplyRotation`도 `_target == null`이면 `ForceRefreshFields()`를 호출하지 않고 바로 반환하도록 분리 — `interactable`을 끄는 시점에 포커스 중이던 필드가 `OnDeselect`로 `OnEndEdit`를 재진입 호출할 수 있는 경우까지 방지.

## Priority 4: 데이터 영속성 및 편의 기능

### InputField 포커스 중 단축키 오작동 방지

`MirrorGizmo`의 Move/Rotate 전환(`wKey`/`eKey`)과 `MirrorPlacementController`의 배치 취소(`escapeKey`)는 New Input System의 `Keyboard.current`로 물리 키 상태를 직접 폴링하는데, 이 방식은 TMP_InputField가 포커스를 갖고 있어도 자동으로 막히지 않음 — 예를 들어 Inspector의 Name/Description 필드에 "west"라고만 타이핑해도 `w`에서 기즈모가 Move 모드로 전환되는 실제 버그가 있었음. `Assets/Scripts/InputFocusGuard.cs`(정적 유틸리티, `EventSystem.current.currentSelectedGameObject`에 `TMP_InputField`가 붙어있는지로 판별)를 두 스크립트의 키보드 단축키 체크 앞단에 공통으로 적용해 해결. 마우스 기반 취소(우클릭)는 타이핑과 충돌하지 않으므로 가드 대상에서 제외.

### 카메라 뷰 컨트롤러 — 오빗 대신 로컬 회전

처음엔 고정 피벗을 중심으로 도는 오빗 카메라를 검토했으나, 피벗에 종속되면 나중에 다른 조작(이동 등)을 얹을 때 "항상 피벗을 바라보기" 제약을 다시 풀어야 해서 확장성이 떨어짐. 대신 `CameraViewController`는 카메라 자신의 로컬 회전만 다룸 — 우클릭 드래그 중 마우스 델타로 `yaw`/`pitch`를 매 프레임 누적하고, 그 값으로 `Quaternion.Euler(pitch, yaw, 0)`를 새로 세팅하는 방식(`transform.Rotate` 반복 호출은 부동소수점 오차로 롤이 누적되는 문제가 있어 피함). `pitch`는 뒤집히지 않게 `_minPitch`/`_maxPitch`(-80~80)로 clamp. 피벗이 없어지므로 Zoom도 "피벗까지 거리 좁히기"가 아니라 휠 입력만큼 카메라 자신의 `transform.forward` 방향으로 전진/후진(dolly)하는 방식으로 처리. 이동(WASD 등)은 이번 범위에서 제외. `EventSystem.current.IsPointerOverGameObject()`를 `Update()` 최상단에서 한 번만 확인해 Inspector 패널 위에서는 회전·줌 모두 반응하지 않도록 함. Edit/Play 모드와 무관하게 항상 동작(지형 관람용이라 게이팅 불필요).

이동 없이 회전+dolly만 있다 보니 사용자가 지형에서 완전히 벗어난 뷰로 가버리면 되돌아올 방법이 없는 문제가 있어, `Awake()`에서 씬에 배치된 초기 Position/Rotation을 그대로 기억해뒀다가 복귀시키는 `public OnClickResetView()`를 추가. 좌측 상단 `Camera Reset Button`에 연결했는데, 이 버튼 GameObject는 씬에 Image만 추가되고 실제 `Button` 컴포넌트가 빠져 있어서 클릭이 전혀 안 되는 상태였음 — `UnityEngine.UI.Button` 컴포넌트를 추가하고 기존 아이콘 Image를 `m_TargetGraphic`으로 연결한 뒤, 다른 버튼들과 같은 방침대로 `OnClick`을 씬에서 `CameraViewController.OnClickResetView`로 직접 연결.

이후 휠 버튼(middle click) 드래그로 화면을 평행 이동하는 `HandlePan()`을 추가. 카메라의 로컬 `right`/`up` 축 기준으로 마우스 델타의 **반대 방향**으로 이동시켜 "화면을 손으로 잡고 끄는" 느낌을 냄(Blender/Maya 등의 팬 컨벤션과 동일). 이동이 생겼지만 `OnClickResetView()`가 이미 `_initialPosition`을 함께 복원하므로 별도 처리 없이 기존 Reset View 버튼으로 팬한 위치도 그대로 복귀됨.

### MirrorPool 컬렉션 구조 — 활성 List + 비활성 Queue

기존엔 `MirrorPool`이 비활성 거울만 `Stack<PlacedMirror>`로 들고 있어서, Save가 필요로 하는 "지금 배치된 거울 전체 순회"가 불가능했음. 비활성 재사용 방식을 LIFO(`Stack`)에서 FIFO(`Queue`)로 바꾸고, 활성 거울을 별도 `List<PlacedMirror>`로 함께 추적해 `ActiveMirrors`로 읽기 전용 노출 — `PlacedMirror.ActiveCount`(개수만 세는 기존 정적 카운터)와 역할을 분리해 "몇 개인지"와 "누가 활성 상태인지"를 각각 책임지도록 함.

### Mirror Name/Description 필드화와 `gameObject.name` 반영

`DisplayName`/`Description`을 자동 프로퍼티에서 `[SerializeField]` 백킹 필드로 전환(Save 직렬화 대상이자 인스펙터 노출 목적). `DisplayName` setter가 `gameObject.name`도 함께 갱신하도록 해 Hierarchy에서 배치된 거울을 이름으로 바로 식별 가능하게 함 — 값이 비어 있으면 `"Mirror"`로 폴백해 빈 이름이 노출되지 않도록 함. `OnEnable` 초기화 시 `DisplayName`은 `"Mirror {번호}"` 형태로 채우는데, 이 번호는 삭제 후 재배치해도 겹치지 않도록 감소 없이 계속 증가만 하는 정적 카운터(`s_NextDisplayNumber`)에서 옴. `Description`은 자동으로 채울 의미 있는 기본값이 없어 빈 문자열 유지.

### JsonUtility vs Newtonsoft.Json

저장 스키마(`MirrorSaveData`)가 `Vector3`/`Quaternion`/`string`으로만 구성된 플랫한 DTO 리스트라 `JsonUtility`만으로 충분하다고 판단. `JsonUtility`는 `Vector3`/`Quaternion`을 컨버터 없이 그대로 직렬화하는 반면, Newtonsoft는 이런 상황에서 이점 없이 별도 패키지 의존성만 늘어남 — Dictionary·다형성 등 복잡한 스키마가 필요해지면 그때 재검토.

### Load를 버튼이 아닌 부트스트랩 씬(`Init`)에서 처리

Load는 "앱 실행 시 항상 최초로 거치는 화면"이라는 요구사항 때문에 Save/Clear와 달리 버튼 클릭이 아니라 별도 씬 `Assets/Scenes/Init.unity`의 `LoadManager.Awake()`에서 처리. 저장 파일이 없거나(`File.Exists` 실패) 파싱이 깨지면(`JsonUtility.FromJson` 예외) 둘 다 `null`로 귀결시켜 예외 없이 빈 상태로 게임 씬에 진입 — 저장 파일은 사용자가 직접 건드릴 수도 있는 외부 입력 경계이므로 방어적으로 처리.

### 저장 파일이 없을 때 StreamingAssets 프리셋 폴백

저장 파일이 아예 없는 최초 실행 상태에서는 거울이 하나도 없는 빈 화면으로 시작해, 평가자가 첫 실행에서 아무 것도 볼 게 없는 문제가 있음. `Assets/StreamingAssets/mirrors.json`에 미리 구성해둔 배치를 프리셋으로 두고, `LoadManager.TryLoad()`가 `persistentDataPath`(`SaveManager.SavePath`)에 저장 파일이 없을 때만 이 프리셋(`SaveManager.PresetPath`)을 대신 읽도록 폴백을 추가. 저장 스키마(`MirrorSaveDataList`)를 그대로 재사용하므로 `GameManager.SpawnLoadedMirrors()`는 프리셋과 실제 저장 데이터를 구분하지 않고 동일하게 처리.

`Application.streamingAssetsPath`는 이 프로젝트가 타깃하는 Windows 스탠드얼론/에디터에서는 일반 파일 경로라 `File.Exists`/`File.ReadAllText`로 동기 접근이 가능함. Android/WebGL은 StreamingAssets가 APK 내부나 압축 스트림으로 패키징되어 `UnityWebRequest` 비동기 접근이 강제되므로, 해당 플랫폼을 타깃하게 되면 `LoadManager.Awake()`를 코루틴/비동기로 바꿔야 함.

### `MonoSingleton<T>`과 씬 간 데이터 전달

`Init` → 게임 씬 전환 시 Unity가 씬 상태를 초기화하는 문제를, 별도의 정적 홀더 클래스 대신 `LoadManager` 자신을 `DontDestroyOnLoad` 싱글톤으로 만들어 해결 — 파싱된 데이터를 들고 씬 전환에서 살아남은 뒤 `GameManager`가 `LoadManager.Instance.LoadedData`를 읽어가는 구조. 재사용 가능하도록 제네릭 `MonoSingleton<T>` 베이스 클래스로 분리(`Assets/Scripts/MonoSingleton.cs`). `Destroy()`가 프레임 끝까지 지연 실행되는 특성 때문에, 하위 클래스가 실제 동작 코드를 실행하기 전 `Instance != this` 체크로 자신이 중복 인스턴스로 걸러졌는지 반드시 확인. 처음엔 읽은 뒤 값을 비우는 `ConsumeLoadedData()`로 1회성 소비를 보장했으나, 현재 코드베이스엔 게임 씬을 `Init` 없이 재로드하는 경로 자체가 없어 그 방어가 실제로 걸리는 상황이 없다고 판단해 걷어내고 단순 읽기 전용 프로퍼티로 되돌림(YAGNI) — 재시작 기능이 추가되면 재검토.

실제 거울 스폰(`MirrorPool.Get()` 호출) 주체는 `GameManager.Start()`로 결정 — `MirrorPlacementController.PlaceMirror()`가 이미 쓰는 `Get()` 패턴과 `MaxMirrorCount` 캡을 그대로 재사용.

### Save/Load 모드 게이팅을 하지 않은 이유

`SaveManager.OnClickSave()`는 처음엔 다른 편집 액션처럼 Edit 모드 체크를 넣었으나, Save 버튼이 Play/Edit 모드 구분 없이 상시 노출되는 UI로 확정되면서 제거 — Save는 현재 상태를 읽기만 하는 비파괴적 동작이라 모드와 무관하게 항상 허용해도 무방하고, 덕분에 `GameManager` 의존성도 사라짐. Load는 애초에 버튼이 아니라 부트스트랩 시점에 1회 자동 실행되므로 모드 개념 자체가 관여하지 않음.

### 에디터에서 매번 `Init`을 거쳐야 하는 번거로움

`Init`이 항상 최초 진입 씬이 되도록 Build Settings에 0번으로 등록했지만, 에디터에서 게임 씬(`Scene.unity`)을 열어놓고 작업하다 Play를 누르면 `Init`을 거치지 않아 Load 흐름이 테스트되지 않는 문제가 있음. `Assets/Editor/PlayModeStartSceneSetup.cs`(`[InitializeOnLoad]` 정적 생성자)로 `EditorSceneManager.playModeStartScene`을 자동으로 `Init.unity`로 지정해 해결 — 에디터 UI를 수동으로 찾아 설정하는 대신 스크립트로 커밋해두면, 이 프로젝트를 처음 여는 사람도 별도 설정 없이 빌드와 동일한 진입 흐름(`Init` → `Scene`)으로 테스트하게 됨. 이미 값이 설정돼 있으면 덮어쓰지 않아 수동으로 다른 씬을 지정해둔 경우를 존중.

### 게임 씬을 `LaserTest.unity`에서 `Scene.unity`로 통합

기존에 템플릿 잔재였던 빈 `Scene.unity`와 실제 게임 로직이 들어있던 `LaserTest.unity` 두 씬이 따로 존재했는데, 하나로 합치면서 `LaserTest.unity`는 삭제하고 `Scene.unity`가 실제 게임 씬 역할을 대체함. 이 과정에서 `Scene.unity`가 새 asset guid를 받게 되어, `ProjectSettings/EditorBuildSettings.asset`에 등록된 두 번째 씬 항목의 `path`는 `Scene.unity`로 갱신됐지만 `guid`는 예전 `LaserTest.unity`의 guid가 그대로 남아 참조가 끊겨 있던 문제가 있었음 — `Scene.unity.meta`의 guid로 맞춰 수정. `LoadManager._gameSceneName`(Init 씬에 직렬화된 값과 코드 기본값 둘 다)도 `"Scene"`으로 갱신.

### Delete/Clear를 `SaveManager`가 아닌 `MirrorSelectionController`에 둔 이유

Clear는 처음엔 Save/Load와 한 세트로 묶여 있던 Roadmap 항목이라 `SaveManager`에 `OnClickClear()`를 넣었으나, Clear는 저장 파일을 전혀 건드리지 않고 **런타임 배치만 초기화**하는 동작(지운 걸 영구 반영하려면 별도로 `Save`를 눌러야 함)이라는 게 명확해지면서 `SaveManager`의 책임(파일 I/O)과 안 맞는다고 판단해 옮김. `MirrorSelectionController`는 `SelectedMirror`를 이미 소유하고 있고, 단일 삭제(`Delete`)를 구현하며 `_mirrorPool`/`_gameManager` 참조를 이미 갖추게 됐으므로 전체 삭제(`Clear`)도 같은 곳에 두면 새 참조 없이 재사용 가능 — `SaveManager`는 다시 `_mirrorPool` 하나만 참조하는 순수 저장 담당으로 남음.

### Delete — 버튼과 단축키가 같은 메서드로 수렴

`[Delete]` 버튼(`OnClickDelete`)과 `Delete` 키 단축키(`Keyboard.current.deleteKey`, `MirrorGizmo`의 W/E 처리와 동일하게 `InputFocusGuard.IsInputFieldFocused()`로 InputField 편집 중엔 무시)가 모두 같은 `DeleteSelected()`로 수렴. 삭제 시 `Select(null)`을 **먼저** 호출해 `SelectionChanged`가 먼저 발행되게 한 뒤(→ `MirrorGizmo`/`MirrorInspectorController`가 이번 프레임에 이미 `_target == null`로 반응) `MirrorPool.Release()`로 실제 반환 — 죽은 참조가 한 프레임이라도 남지 않도록 순서를 고정.

### Exit Button — 에디터/빌드 분기

원래 기능 없이 자리만 차지하던 `Setting Button`을 `Exit Button`으로 재활용. `Application.Quit()`은 에디터의 Play 모드에서는 아무 동작도 하지 않아(빌드에서만 실제로 종료됨) 에디터에서 테스트할 때 버튼이 고장난 것처럼 보이는 문제가 있음 — `GameManager.OnClickExit()`에서 `#if UNITY_EDITOR`로 분기해 에디터에서는 `EditorApplication.isPlaying = false`로 Play 모드를 종료하고, 빌드에서만 `Application.Quit()`을 호출하도록 처리.

### `CanvasGroupAlphaLoop` — DOTween 도입과 `Motions/` 폴더 분리

모드 인디케이터 아이콘에 알파를 반복시키는 모션을 넣기 위해 `Assets/Plugins/Demigiant/DOTween`을 프로젝트에 추가(설치 과정에서 `ProjectSettings`에 `DOTWEEN` 스크립팅 정의 심볼과 `Assets/Resources/DOTweenSettings.asset`이 자동 생성됨).

처음엔 이 로직을 `ModeIndicator`에 직접 넣었으나, `ModeIndicator.ApplyColor()`가 `Image.color`를 통째로 덮어써서 알파를 함께 건드리는 트윈과 매 모드 전환마다 충돌하는 문제가 있었음 — `Image.color`의 알파 대신 별도 `CanvasGroup.alpha`로 불투명도를 분리하니 색상(모드 전환)과 알파(루프 모션)가 서로 독립적인 채널이 되어 충돌이 사라짐. 이 김에 알파 루프 자체도 `ModeIndicator`(모드에 따른 색상·텍스트 표시 책임)에서 완전히 분리해 `CanvasGroupAlphaLoop`라는 범용 컴포넌트로 뽑아냄 — 어떤 UI 요소든 `CanvasGroup`만 붙어 있으면 재사용 가능해짐. 모션 관련 컴포넌트가 늘어날 것을 대비해 `Assets/Scripts/Motions/` 폴더로 분리 관리하기 시작.

`[RequireComponent(typeof(CanvasGroup))]`로 이 컴포넌트가 `CanvasGroup` 없이는 존재할 수 없도록 강제하고, `_canvasGroup` 필드는 씬에서 수동으로 드래그하는 대신 `Awake()`에서 `GetComponent<CanvasGroup>()`으로 캐싱 — `RequireComponent`가 있으니 항상 성공이 보장되고, Inspector엔 실제로 조절해야 하는 `_minAlpha`/`_maxAlpha`/`_duration`만 노출됨. `OnDestroy()`에서 트윈을 `Kill()`해 오브젝트 파괴 시 DOTween 콜백이 죽은 대상에 접근하는 것을 방지.

### `GameConfig` — 설정값을 한 곳에서 관리

`LaserEmitter`의 최대 반사 횟수와 `MirrorPlacementController`의 최대 거울 개수가 각자 다른 클래스에 박힌 `const`였음 — 값을 바꾸려면 코드를 고쳐야 했고, 같은 값을 참조하는 `MirrorCountDisplay`/`GameManager`(Load 시 캡 체크)는 `MirrorPlacementController.MaxMirrorCount`를 정적으로 갖다 쓰는 간접 참조였음. 이 둘을 `GameConfig`(`ScriptableObject`) 하나로 옮겨 값 하나만 바꾸면 모든 참조처에 즉시 반영되게 함.

처음엔 각 소비 클래스가 `[SerializeField] private GameConfig _gameConfig;`를 들고 씬에서 같은 에셋을 4곳에 일일이 드래그하는 방식으로 갔으나, 반복 연결이 번거롭다는 이야기가 나와 재검토. "이미 있는 `Init` 씬의 `LoadManager`(`DontDestroyOnLoad` 싱글톤)에 얹으면 어떨까"도 고려했지만 기각 — `LoadManager`는 "저장 데이터를 읽어 씬 전환 간 들고 있다가 넘겨준다"는 책임 하나만 가진 클래스라 레이저/거울 같은 게임 밸런스 값을 얹으면 책임이 섞이고, 무엇보다 `LoadManager.Instance`는 `Init`을 거쳐야만 존재하는데 이 값들은 `Init` 여부와 무관하게 항상 필요한 핵심 설정이라(개발 중 `Scene`을 바로 재생하는 흔한 워크플로에서 곧바로 깨짐) 가용성 문제가 있었음.

최종적으로 `GameConfig` 자신을 `Assets/Resources/`에 두고 `GameConfig.Instance`(내부적으로 `Resources.Load<GameConfig>("GameConfig")`, 최초 1회만 로드해 캐싱)로 접근하는 정적 싱글톤 에셋 패턴을 채택 — 어떤 스크립트도 `[SerializeField]` 참조나 씬 내 위치와 무관하게 `GameConfig.Instance.MaxMirrorCount`처럼 바로 쓸 수 있어 연결 자체가 필요 없어짐. 이미 `Assets/Resources/DOTweenSettings.asset`이 같은 폴더 관례를 쓰고 있어 이 프로젝트에 낯선 패턴도 아님. `LaserEmitter`는 `_maxReflectionCount`로 배열 크기(`_linePositions`)를 잡는데, 필드 초기화 시점엔 아직 `GameConfig.Instance`를 참조할 이유가 없어(정적 프로퍼티라 시점 문제 없음) 그대로 `Awake()`에서 읽어와 배열을 할당.

### `GameManager.Mode`/`ModeChanged` static 전환

`MirrorPlacementController`, `MirrorSelectionController`, `EditModeOnly`, `MirrorInspectorController`, `MirrorGizmo`, `ModeButtonGroup` 6개 클래스가 전부 `[SerializeField] private GameManager _gameManager;`를 들고 `_gameManager.Mode`/`_gameManager.ModeChanged`만 읽는 형태였음 — `GameConfig`를 정리하면서 같은 반복 연결 문제가 `GameManager`에도 있다는 게 눈에 띔.

`GameConfig`처럼 별도 애셋으로 뺄 값이 아니라 애초에 `GameManager` 자신이 소유해야 하는 런타임 상태(모드 전환은 `OnClickEditButton`/`OnClickPlayButton` 같은 씬 버튼 바인딩이 필요해 인스턴스로 남아야 함)라, `PlacedMirror.ActiveCount`/`ActiveCountChanged`가 이미 쓰고 있는 것과 같은 패턴으로 `Mode`/`ModeChanged`만 `static`으로 전환 — `GameManager` 인스턴스는 씬에 그대로 있고 `_modeIndicator`/`_mirrorPool`도 그대로 갖고 있지만, 다른 6개 클래스는 `GameManager.Mode`/`GameManager.ModeChanged`로 바로 접근해 `_gameManager` 필드 자체가 필요 없어짐. 씬에 `GameManager`가 항상 하나뿐이라 static 상태 공유로 인한 충돌 위험은 없음.

### `MirrorGizmo.Mode`/`ModeChanged`도 같은 패턴으로

`MirrorGizmo`의 `Mode`/`ModeChanged`(타입 `GizmoHandleKind`)를 참조하는 곳은 `GizmoModeToggle` 하나뿐이라, `GameManager` 때와 달리 static으로 바꿔도 "필드 연결이 사라지는" 실익은 거의 없음 — `GizmoModeToggle`은 `OnClickPosition()`/`OnClickRotation()`에서 `_gizmo.SetMode(...)`도 호출해야 해서 `_gizmo` 참조 자체는 계속 들고 있어야 함. 그럼에도 가독성·일관성 관점에서 진행: `GameManager.SetMode()`도 이미 "인스턴스 메서드가 static 필드를 바꾸는" 구조라, `MirrorGizmo`도 정확히 같은 모양(`s_Mode` static 필드, `Mode`/`ModeChanged` static, `SetMode()`는 `ApplyMode()`에서 인스턴스 소유 `_handles`를 건드려야 하니 인스턴스 메서드로 유지)으로 맞추면 두 클래스가 같은 문법으로 읽혀 한쪽을 이해하면 다른 쪽도 바로 이해됨. `GizmoModeToggle`도 `Mode` 읽기·`ModeChanged` 구독은 static으로, `SetMode()` 호출만 `_gizmo` 인스턴스로 남김.
