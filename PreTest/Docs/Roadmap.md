# 📅 개발 우선순위 및 스케줄 (Roadmap)

### 🔴 Priority 1: 코어 레이저 및 수신기 시스템 (핵심 판정)

> **중요도:** 최상 (과제의 뼈대이자 가장 먼저 검증되어야 할 로직)
> 
- **주요 작업:**
    - `LaserEmitter` 구현: `While` 루프 기반 `Physics.Raycast`, 최대 10회 반사 및 `LineRenderer` 시각화 연동.
    - 레이저 안전장치: Self-Collision 방지 오프셋 적용 및 앞/뒷면 충돌 판별(`Vector3.Dot` 활용).
    - `ILaserHitReceiver` 인터페이스 정의 및 `LaserReceiver` 구현 (`LateUpdate` 기반 `isHitThisFrame` 자동 복귀).
- **목표:** 거울이 없어도 레이저가 벽에 부딪혀 꺾이고 수신기를 켰다 끄는 기본 동작이 완벽히 작동하는지 확인.

### 🟡 Priority 2: 거울 배치 및 조작 시스템 (핵심 UX)

> **중요도:** 상 (사용자가 직접 상호작용하는 핵심 기능)
> 
- **주요 작업:**
    - `MirrorController` 프리팹 구성 (Rigidbody 제외, Collider 설정).
    - Raycast + Surface Normal 정렬을 이용한 지형/벽면 스냅 배치 시스템 구현.
    - 거울 최대 100개 제한 로직 및 중첩(Overlap) 배치 방지 예외 처리.
    - Gizmo 드래그를 통한 표면 수직축 기준 회전 기능 구현.

### 🟢 Priority 3: UI 구조 및 양방향 동기화 (완성도 향상)

> **중요도:** 중상 (툴로서의 완성도와 가산점 확보)
> 
- **주요 작업:**
    - Main Canvas 및 하위 Sub-Canvas(`InspectorPanelGroup`) 구성 (Canvas Rebatching 최적화).
    - `GameManager`를 통한 `Edit` ↔ `Play` 모드 상태 관리 구현.
    - 선택된 거울의 Position/Rotation 값을 보여주는 Inspector UI 구현 및 Gizmo와의 **양방향 데이터 바인딩**.
    - Play 모드 시 Inspector UI를 Read-Only(조회 전용)로 전환하는 로직 처리.

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