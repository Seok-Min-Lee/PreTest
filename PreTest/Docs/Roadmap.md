# 📅 개발 우선순위 및 스케줄 (Roadmap)

> 각 항목의 상세 구현 배경·트레이드오프는 `Docs/ImplementationNotes.md` 참고.

### 🔴 Priority 1: 코어 레이저 및 수신기 시스템 (핵심 판정)

> **중요도:** 최상 (과제의 뼈대이자 가장 먼저 검증되어야 할 로직)
> 
- **주요 작업:**
    - `LaserEmitter` 구현: `While` 루프 기반 `Physics.Raycast`, 최대 10회 반사 및 `LineRenderer` 시각화 연동.
    - 레이저 안전장치: Self-Collision 방지 오프셋 적용 및 앞/뒷면 충돌 판별.
    - `ILaserReflector` 인터페이스 정의 및 `MirrorController` 구현: 반사체 타입 확장을 대비한 디커플링.
    - `ILaserHitReceiver` 인터페이스 정의 및 `LaserReceiver` 구현 (`LateUpdate` 기반 `isHitThisFrame` 자동 복귀).
- **목표:** 거울이 없어도 레이저가 벽에 부딪혀 꺾이고 수신기를 켰다 끄는 기본 동작이 완벽히 작동하는지 확인.

### 🟡 Priority 2: 거울 배치 및 조작 시스템 (핵심 UX) — ✅ 완료

> **중요도:** 상 (사용자가 직접 상호작용하는 핵심 기능)
> 
- **주요 작업:**
    - `MirrorController` 프리팹 구성 (Rigidbody 제외, Collider 설정).
    - `MirrorPool`을 통한 오브젝트 풀링: `Instantiate`/`Destroy` 대신 `SetActive(true/false)`로 재사용.
    - `Floor` 레이어 콜라이더 전체에 대해 격자 스냅 없는 자유 배치: `Add Mirror` → 배치 모드 진입 → 표면 법선 정렬 고스트 프리뷰 → 클릭으로 확정.
    - 배치 모드 취소: 마우스 우클릭 또는 `Esc` 키로 고스트를 숨기고 즉시 종료.
    - 거울 최대 100개 제한 및 좌측 하단 현재/최대 개수 표시.
    - `MirrorGizmo`를 통한 선택 거울 조작: 축별 핸들(Move X/Y/Z 큐브 + Rotate X/Y/Z 링), `Edit Option` 버튼과 양방향 동기화.
    - `Floor`/`Mirror`/`GizmoHandle` 레이어 분리로 배치·선택·레이저 레이캐스트 간 상호 간섭 방지.

### 🟢 Priority 3: UI 구조 및 양방향 동기화 (완성도 향상) — ✅ 완료

> **중요도:** 중상 (툴로서의 완성도와 가산점 확보)
> 
- **주요 작업:**
    - Main Canvas 및 하위 Sub-Canvas(`InspectorPanelGroup`) 구성 (Canvas Rebatching 최적화).
    - `GameManager`를 통한 `Edit` ↔ `Play` 모드 상태 관리 구현: `ModeChanged` 이벤트를 배치/기즈모/버튼 표시가 각각 구독해 게이팅.
    - 선택된 거울의 Position/Rotation/Name/Description을 보여주는 Inspector UI 구현 및 Gizmo와의 **양방향 데이터 바인딩**.
    - Play 모드 시, 그리고 선택된 거울이 없을 때 Inspector UI를 Read-Only(조회 전용)로 전환.
    - Inspector 패널 하단 `Close Button`으로 닫고, 거울 클릭 시 다시 열리도록 처리.
    - Button `OnClick`/InputField `OnEndEdit` 등 UI 이벤트는 코드 대신 씬에서 직접 연결하는 방식으로 통일.

### 🔵 Priority 4: 데이터 영속성 및 편의 기능 (디테일 마감)

> **중요도:** 중 (레벨 에디터 및 사용자 편의성 극대화)
> 
- **주요 작업:**
    - `SaveManager` 구현: JSON 직렬화(`JsonUtility`)를 통한 거울 배치 정보 `Save / Load / Clear` 연동.
    - 선택된 거울 삭제 기능 (`[Delete]` 버튼 및 `Delete` 단축키).
    - InputField 포커스 시 단축키 오작동 방지 예외 처리(`EventSystem` 연동). — ✅ `InputFocusGuard`로 구현.
    - 3D L자 지형 관람을 위한 마우스 Orbit(회전) 및 Zoom 카메라 컨트롤러 추가. — ✅ `CameraViewController`로 구현. 좌측 상단 `Camera Reset Button`으로 초기 위치/회전 복귀 기능도 함께 추가.

### ⚫ Priority 5: 최종 검증 및 문서화 (제출 준비)

> **중요도:** 상 (평가자에게 기술력을 어필하는 마지막 관문)
> 
- **주요 작업:**
    - 전체 시나리오 통합 테스트 (배치 ➔ 저장 ➔ Play 모드 검증 ➔ Edit 복귀 등 에지 케이스 점검).
    - Git 커밋 로그 및 브랜치 정리 (Conventional Commits 규칙 준수).
    - **README.md 작성:** 아키텍처 설계 이유(인터페이스, Sub-Canvas, Raycast 정밀 배치 등) 상세 기술.
