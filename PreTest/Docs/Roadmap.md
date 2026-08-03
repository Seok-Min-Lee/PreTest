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
    - `FloorGrid` 기반 바닥 그리드 클릭 배치 구현: 좌측 하단 `Add Mirror` 버튼 → 배치 모드 진입 → 커서가 위치한 그리드 셀에 반투명 고스트 프리뷰(배치 가능 시 초록/불가 시 빨강) 표시 → 클릭으로 확정.
        - (구현 노트) 최초 로드맵의 "Raycast + Surface Normal 정렬 지형/벽면 스냅"은 요구사항을 구체화하는 과정에서 스타크래프트 건물 배치 방식의 "바닥 그리드 클릭 배치"로 범위가 좁혀짐. 벽면 스냅은 이번 범위에서 제외하고 수평 바닥 그리드만 지원.
    - 거울 최대 100개 제한(`FloorGrid.IsFull`, 도달 시 `Add Mirror` 버튼 비활성화) 및 중첩 배치 방지(그리드 점유 Dictionary로 이미 점유된 셀은 배치 불가 처리).
    - `MirrorGizmo`를 통한 선택 거울 조작: Move 핸들 드래그로 바닥 평면 자유 이동(그리드 스냅 없음, 드래그 시작 시 기존 점유 셀 해제), Rotate 핸들 드래그로 바닥 수직축(Y) 기준 회전만 허용.
        - (구현 노트) 이동/회전 드래그는 `Physics.Raycast`가 아닌 `Plane.Raycast`(평면-광선 수학 교차)로 계산해 다른 거울의 콜라이더에 드래그가 끊기지 않도록 처리. Rotation은 Yaw만 갱신해 X/Z 축은 항상 고정.
    - `Floor`/`Mirror`/`GizmoHandle` 레이어 분리로 배치·선택·레이저 레이캐스트 간 상호 간섭 방지 (`LaserEmitter`도 `GizmoHandle` 레이어를 제외하도록 처리).

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