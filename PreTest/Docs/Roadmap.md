# 📅 개발 우선순위 및 스케줄 (Roadmap)

### 🔴 Priority 1: 코어 레이저 및 수신기 시스템 (핵심 판정)

> **중요도:** 최상 (과제의 뼈대이자 가장 먼저 검증되어야 할 로직)
> 
- **주요 작업:**
    - `LaserEmitter` 구현: `While` 루프 기반 `Physics.Raycast`, 최대 10회 반사 및 `LineRenderer` 시각화 연동.
    - 레이저 안전장치: Self-Collision 방지 오프셋 적용 및 앞/뒷면 충돌 판별.
        - (구현 노트) 볼록 콜라이더는 항상 진입면(레이저 방향과 반대인 면)만 히트되므로 `Vector3.Dot(direction, hit.normal)` 비교만으로는 앞/뒷면을 구분할 수 없음. 대신 `ILaserReflector.ReflectiveNormal`(반사체가 정의한 정면 벡터)과 `hit.normal`을 임계값(`FrontFaceDotThreshold`) 기반으로 비교하여, 반사체가 설계한 정면 방향과 일치할 때만 반사되도록 처리.
    - `ILaserReflector` 인터페이스 정의 및 `MirrorController` 구현: `LaserEmitter`가 `MirrorController`를 직접 참조하지 않고 `GetComponent<ILaserReflector>()`로 조회하도록 디커플링해 향후 다른 반사체 타입 확장에 대비.
    - `ILaserHitReceiver` 인터페이스 정의 및 `LaserReceiver` 구현 (`LateUpdate` 기반 `isHitThisFrame` 자동 복귀).
- **목표:** 거울이 없어도 레이저가 벽에 부딪혀 꺾이고 수신기를 켰다 끄는 기본 동작이 완벽히 작동하는지 확인.

### 🟡 Priority 2: 거울 배치 및 조작 시스템 (핵심 UX) — ✅ 완료

> **중요도:** 상 (사용자가 직접 상호작용하는 핵심 기능)
> 
- **주요 작업:**
    - `MirrorController` 프리팹 구성 (Rigidbody 제외, Collider 설정).
    - `MirrorPool`을 통한 오브젝트 풀링: `Instantiate`/`Destroy` 대신 `SetActive(true/false)`로 재사용. 배치 시 풀에 남은 비활성 인스턴스가 있으면 그것을 재사용하고, 없을 때만 새로 `Instantiate`. 나중에 삭제 기능을 구현할 때도 `Destroy` 대신 `MirrorPool.Release()`로 반납하는 방식으로 이어감.
        - (구현 노트) `PlacedMirror.ActiveCount`는 재사용 시 다시 실행되지 않는 `Awake`/`OnDestroy` 대신 `SetActive`마다 호출되는 `OnEnable`/`OnDisable`로 카운트하도록 변경 — 그래야 풀에서 꺼내 쓸 때도 활성 개수가 정확히 갱신됨.
    - `Floor` 레이어 콜라이더 전체(평지·경사면 구분 없이)에 대해 격자 스냅 없는 자유 배치: 좌측 하단 `Add Mirror` 버튼 → 배치 모드 진입 → 커서가 위치한 지점에 `hit.normal` 기반 표면 법선 정렬 고스트 프리뷰 표시 → 클릭으로 확정.
        - (구현 노트) 최초 로드맵의 "Raycast + Surface Normal 정렬"에서 한 차례 `FloorGrid` 기반 바닥 그리드 클릭 배치(스타크래프트 건물 배치 방식)로 좁혀졌었으나, Inspector에서 Position/Rotation을 자유 편집하는 기능을 준비하며 그리드 점유 정보가 실제 위치와 어긋나는 문제를 피하기 위해 초기 배치 자체를 다시 전체 자유 배치로 통일함. `FloorGrid.cs`는 이 과정에서 삭제.
    - 배치 모드 취소: 배치 모드 진입 중 마우스 우클릭 또는 `Esc` 키 입력 시 고스트를 숨기고 배치 모드를 즉시 종료(`MirrorPlacementController.CancelPlacement`).
    - 거울 최대 100개 제한: 그리드 점유 개수가 아니라 `PlacedMirror.ActiveCount`(정적 카운터, `OnEnable`/`OnDisable`에서 증감)로 씬 전체 거울 개수를 기준으로 판정, 도달 시 `MirrorPlacementController.BeginPlacement`가 조기 반환되어 배치 모드 자체가 시작되지 않음. 중첩 배치 방지는 하지 않음(자유 배치 특성상 허용).
        - (구현 노트) 좌측 하단 "현재 수 / 최대 수" 텍스트는 `MirrorCountDisplay`가 `PlacedMirror.ActiveCountChanged`(정적 이벤트, `OnEnable`/`OnDisable`에서 발생) 구독해서 갱신. `MirrorPlacementController.MaxMirrorCount`도 표시를 위해 `public`으로 열어둠. 도달 시 `Add Mirror` 버튼을 `interactable = false`로 비활성화하는 처리도 한때 있었으나, `BeginPlacement`의 조기 반환으로 이미 기능적으로 막혀 있고 이 카운트 텍스트가 시각적 피드백을 대신하므로 제거함.
    - `MirrorGizmo`를 통한 선택 거울 조작: 생성 시 표면 도킹과는 별개로, 선택 후 편집은 유니티 에디터 Move/Rotate 툴처럼 **축별 핸들**(Move X/Y/Z 큐브 3개 + Rotate X/Y/Z 링 3개, `GizmoHandle._axis`로 구분)로 처리. 각 핸들은 거울의 로컬 축(`transform.right`/`up`/`forward`) 기준으로만 이동/회전되어, 표면 제약 없이 정확히 그 축만 조작 가능. 색상은 유니티 관례를 따라 X=빨강/Y=초록/Z=파랑(`Assets/Materials/GizmoAxisX·Y·Z.mat`).
        - (구현 노트) Move는 "카메라 방향과 선택된 축을 함께 포함하는 평면"에 `Plane.Raycast`로 교차시킨 뒤 그 교차점을 축 위로 투영해 그랩 지점 기준 델타만큼 이동시키는 방식(점프 없음). Rotate는 그 축을 법선으로 한 평면 위에서 시작 방향과 현재 방향의 `Vector3.SignedAngle`을 구해 회전시키는 방식 — 예전에는 이 축이 표면 법선 하나로 고정돼 있었는데, 이제는 선택한 핸들의 축으로 파라미터화됨.
        - (구현 노트) Rotate 핸들은 회전축을 직관적으로 보여주기 위해 구체 대신 `Assets/Models/ring.fbx` 토러스 메시로 교체(재질은 기존 축 색상 그대로 유지). 유니티 임포터가 안정적 해시 기반 `fileID`(`fileIdsGeneration: 2`)를 쓰는 탓에 씬 텍스트를 직접 편집해 메시를 연결할 수 없어, `Assets/Editor/GizmoRotateHandleSetup.cs`(메뉴: Tools > Gizmo > Rotate 핸들에 Ring 메시 적용)로 에디터에서 정확히 연결. 판정용 `Collider`도 `SphereCollider`에서 `MeshCollider`(non-convex)로 교체 — convex를 켜면 링 가운데 빈 구멍이 볼록 껍질로 메워져 클릭 판정이 부정확해지므로, 레이캐스트 전용 정적 콜라이더 특성을 살려 non-convex로 실제 링 형태를 그대로 유지.
        - (구현 노트) Move 핸들(원점 기준 오프셋 배치)과 Rotate 핸들(원점을 감싸는 링)이 동시에 노출되면 위치가 겹쳐 레이캐스트 판정이 꼬일 수 있어, 유니티 에디터의 `W`(Move)/`E`(Rotate) 단축키처럼 `MirrorGizmo._mode` 하나로 두 세트 중 한쪽만 활성화하도록 분리. 드래그 중(`_draggingHandle != null`)에는 입력을 무시해 조작 도중 모드가 바뀌지 않도록 함.
        - (구현 노트) 좌측 상단 `Edit Option`의 `Position`/`Rotation` 버튼도 이 모드를 양방향으로 조작·표시하도록 `GizmoModeToggle`로 연동. `MirrorGizmo.SetMode`를 `public`으로 열고 모드가 바뀔 때 `ModeChanged` 이벤트를 쏘도록 해, 키보드(W/E)든 버튼 클릭이든 같은 진입점을 거치게 함. 각 버튼 위에 미리 준비된 `cover` 이미지(버튼 전체를 덮는 하이라이트, 클릭은 `Selectable`이 부모 계층까지 훑어 여전히 버튼에서 받음)를 현재 모드에 해당하는 쪽만 `SetActive(true)`로 켜서 시각적으로 동기화.
    - `Floor`/`Mirror`/`GizmoHandle` 레이어 분리로 배치·선택·레이저 레이캐스트 간 상호 간섭 방지 (`LaserEmitter`도 `GizmoHandle` 레이어를 제외하도록 처리).

### 🟢 Priority 3: UI 구조 및 양방향 동기화 (완성도 향상) — ✅ 완료

> **중요도:** 중상 (툴로서의 완성도와 가산점 확보)
> 
- **주요 작업:**
    - Main Canvas 및 하위 Sub-Canvas(`InspectorPanelGroup`) 구성 (Canvas Rebatching 최적화). — ✅
    - `GameManager`를 통한 `Edit` ↔ `Play` 모드 상태 관리 구현. — ✅ `AppMode`(Edit/Play) 상태와 `Edit Button`/`Play Button` 클릭 연동, 다른 스크립트가 구독할 `ModeChanged` 이벤트, 그리고 이 이벤트를 구독하는 각 기능별 게이팅까지 완료.
        - (구현 노트) `MirrorPlacementController`도 `ModeChanged`를 구독(`OnEnable`/`OnDisable` — 이 컴포넌트는 자기 자신을 끄지 않으므로 `EditModeOnly`와 달리 `Awake`/`OnDestroy`가 아닌 `OnEnable`/`OnDisable`로 충분)해 두 겹으로 게이팅함: `BeginPlacement()` 진입 시점에 `Mode == Edit`가 아니면 조기 반환(버튼이 `EditModeOnly`로 숨겨져 있어 평소엔 호출 자체가 안 되지만 방어적으로 체크), 그리고 배치 도중(`_isPlacing == true`)에 Play로 전환되면 `HandleModeChanged`가 `CancelPlacement()`를 호출해 고스트를 즉시 숨기고 배치를 중단시킴.
        - (구현 노트) 좌측 상단 모드 표시(배경 Image + 아이콘 + TMP 텍스트)는 `GameManager`가 직접 만지지 않고 별도 `ModeIndicator` 클래스로 분리 — `GameManager.SetMode`가 `ModeIndicator.SetMode(mode)`를 호출하면, 배경·아이콘·텍스트 색과 텍스트 내용을 한 번에 갱신(Edit=시안색, Play=`Play Button`과 동일한 초록).
        - (구현 노트) `Edit Button`/`Play Button` 활성화 상태도 `GameManager`가 직접 건드리지 않고 `ModeButtonGroup`이 `GameManager.ModeChanged` 이벤트를 구독해 처리 — 현재 모드에 해당하는 버튼은 `SetActive(false)`로 숨기고 반대쪽만 보이게 함(두 버튼이 같은 자리에 겹쳐 있었기 때문). 기존에 나뉘어 있던 두 버튼 GameObject는 그대로 두고, `ModeButtonGroup` 전용 부모 오브젝트로 감싸서 이 스크립트가 그 부모에 붙도록 구성.
        - (구현 노트) `Add Mirror`/`Delete`/`Edit Option`(Position·Rotation 토글) 버튼처럼 Play 모드에서 아예 안 보여야 하는 UI는 매번 새 클래스를 만들지 않고, 범용 `EditModeOnly` 컴포넌트를 해당 오브젝트에 붙이는 방식으로 통일. `Edit` 모드가 아니면 자기 자신을 `SetActive(false)`. 단, 자기 자신을 끄는 컴포넌트라 `OnEnable`/`OnDisable`로 이벤트를 구독하면 한 번 꺼진 뒤 다시 켜질 신호를 받을 수 없어서, `Awake`/`OnDestroy`로 구독(순수 C# event는 비활성 오브젝트에도 정상 호출됨)해 이 문제를 피함.
    - 선택된 거울의 Position/Rotation 값을 보여주는 Inspector UI 구현 및 Gizmo와의 **양방향 데이터 바인딩**. — ✅ `MirrorInspectorController`로 구현. Position/Rotation 각 X/Y/Z InputField 6개를 `Update`에서 매 프레임 값이 실제로 바뀐 경우에만 갱신(불필요한 문자열 GC 방지), `OnEndEdit`로 입력한 값은 거울 Transform에 반영. InputField는 필드에 포커스가 가 있으면 갱신을 건너뛰어 타이핑 중 값이 덮어써지지 않게 함. `ContentType`도 자유 텍스트(Standard)에서 DecimalNumber로 변경해 숫자 외 입력 자체를 막음.
        - (구현 노트) 정책상 Play 모드에서도 거울 **선택과 Inspector 값 표시는 계속 되어야 하지만**, Delete·InputField 편집·Move/Rotate 기즈모 조작은 막혀야 함 — 이 때문에 "선택 상태"와 "기즈모로 편집 가능한 상태"를 분리하는 리팩터링이 필요했음. `MirrorSelectionController`가 더 이상 `MirrorGizmo`를 직접 참조해 `Attach`/`Detach`를 호출하지 않고, 대신 `SelectedMirror` 프로퍼티와 `SelectionChanged` 이벤트만 노출. `MirrorGizmo`와 `MirrorInspectorController`가 각자 이 이벤트를 구독해서 독립적으로 반응(Gizmo는 Edit 모드일 때만 `SetActive(true)`, Inspector는 모드와 무관하게 항상 값 표시).
        - (구현 노트) `MirrorGizmo`는 씬에서 기본 비활성(`m_IsActive: 0`)으로 시작했는데, 이 상태로는 `Awake`가 아예 실행되지 않아 `SelectionChanged`/`ModeChanged` 구독 자체가 안 걸리는 문제가 있었음(비활성 오브젝트는 처음 활성화되기 전까지 `Awake`가 호출되지 않음). `EditModeOnly`와 같은 이유로, 씬에서 기본 활성 상태로 바꾸고 `Awake` 시점에 구독 후 스스로 `UpdateVisibility()`로 즉시 숨기는 방식으로 변경.
    - Play 모드 시 Inspector UI를 Read-Only(조회 전용)로 전환하는 로직 처리. — ✅ `MirrorInspectorController`가 `GameManager.ModeChanged`를 구독해 8개 InputField의 `interactable`을 토글(Edit만 편집 가능, Play는 값은 보이되 수정 불가).
        - (구현 노트) `Name`/`Description` InputField도 Position/Rotation과 동일한 패턴으로 우선 연동함(데이터 영속 기능은 Priority 4 예정이라 값 자체는 `PlacedMirror.DisplayName`/`Description`이라는 단순 문자열 프로퍼티에만 보관). Position/Rotation과 달리 Gizmo 같은 외부 변경 주체가 없어 `Update`의 매 프레임 변경 감지 대상(`RefreshFieldsIfChanged`)에는 넣지 않고, 선택 변경 시 `ForceRefreshFields`에서만 동기화. 오브젝트 풀링 재사용 시 이전 배치의 값이 남지 않도록 `PlacedMirror.OnEnable`에서 매번 빈 문자열로 초기화.
        - (구현 노트) Inspector 하단 `Close Button`으로 패널을 닫고 거울을 클릭하면 다시 열리도록 `InspectorPanelToggle`을 추가. 패널 GameObject(`Inspector`)에 직접 붙이면 꺼진 동안 이 컴포넌트도 같이 비활성화되어 다시 열어줄 이벤트조차 못 받으므로, 항상 켜져 있는 부모 `Canvas`(Sub-Canvas 루트)에 붙여 `OnEnable`/`OnDisable`로 안전하게 구독. 이미 선택된 거울을 다시 클릭한 경우 `MirrorSelectionController.SelectionChanged`는 (선택 대상이 안 바뀌었으므로) 발생하지 않아 재오픈 신호로 못 쓰기 때문에, "클릭"과 "선택 변경"을 분리한 별도의 `MirrorClicked` 이벤트를 추가해 여기에 연결. Close 버튼의 `OnClick`도 다른 버튼들과 같은 방침대로 코드 `AddListener` 없이 `public OnClickClose()`를 씬에서 직접 연결.
        - (구현 노트) 8개 InputField의 `OnEndEdit`도 코드 `AddListener` 대신 씬에서 직접 연결하는 방식으로 통일. 각 핸들러를 `public void OnEndEditPositionX(string value)` 형태로 열고, TMP_InputField의 `OnEndEdit` 퍼시스턴트 콜을 "Dynamic string"(`m_Mode: 0`, 호출 시점의 실제 입력값을 그대로 전달)으로 연결. `MirrorInspectorController`는 이제 `OnEnable`/`OnDisable`에서 `SelectionChanged`/`ModeChanged` 구독만 담당하고, InputField 자체 참조는 값 표시(`SetTextWithoutNotify`)·상호작용 토글(`interactable`)용으로만 남음.

### 🔵 Priority 4: 데이터 영속성 및 편의 기능 (디테일 마감)

> **중요도:** 중 (레벨 에디터 및 사용자 편의성 극대화)
> 
- **주요 작업:**
    - `SaveManager` 구현: JSON 직렬화(`JsonUtility`)를 통한 거울 배치 정보 `Save / Load / Clear` 연동.
    - 선택된 거울 삭제 기능 (`[Delete]` 버튼 및 `Delete` 단축키).
    - InputField 포커스 시 단축키 오작동 방지 예외 처리(`EventSystem` 연동).
    - 3D L자 지형 관람을 위한 마우스 Orbit(회전) 및 Zoom 카메라 컨트롤러 추가.

### ⚫ Priority 5: 최종 검증 및 문서화 (제출 준비)

> **중요도:** 상 (평가자에게 기술력을 어필하는 마지막 관문)
> 
- **주요 작업:**
    - 전체 시나리오 통합 테스트 (배치 ➔ 저장 ➔ Play 모드 검증 ➔ Edit 복귀 등 에지 케이스 점검).
    - Git 커밋 로그 및 브랜치 정리 (Conventional Commits 규칙 준수).
    - **README.md 작성:** 아키텍처 설계 이유(인터페이스, Sub-Canvas, Raycast 정밀 배치 등) 상세 기술.