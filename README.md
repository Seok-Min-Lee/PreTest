
# Deepfine 입사 지원자 대상 사전 과제

Edit. 2025-12-17

<details>
<summary>과제 내용</summary>
<div markdown="1">

## 1. 개요
최근 AI 활용이 보편화되면서 단순 코드 완성도만으로는 평가가 어렵습니다.  
본 과제는 구현 과정에서 드러나는 **프로그래밍 스타일, 구조 설계, 확장 가능성, 디자인 패턴 활용, 디버깅/테스트 관점** 등을 종합적으로 확인하기 위한 목적입니다.
하여 요구 사항대로 정상 작동과 함께 프로젝트 구성에 중점을 두고 있습니다.

## 2. 목표
플레이 타임에 Mirror(Prefab) 를 설치/조작하여, Laser가 발사되어 Receiver까지 도달하도록 시스템을 구현합니다.
  
## 3. 요구사항
### 공통
- 프로젝트 내 Laser, Mirror, Receiver에 자유롭게 아래 요구 사항을 구현합니다.
- 사용 된 Unity는 6000.2.7f2
### Laser
- Collider가 있는 GameObject에 닿으면 충돌 지점까지 그려져야 합니다.
- Mirror에 닿을 시 정반사가 됩니다.
- 반사는 최대 10회까지 수행 되도록 제약 조건을 구현하여야 합니다.
### Receiver
- Laser가 Receiver에 닿으면 상태 변화가 발생해야 합니다. (상태 변화는 색상, 연출 등 자유)
- Laser가 닿지 않으면 원래 상태로 자동 복귀해야 합니다.
### Mirror
- Mirror는 플레이 타임에 동적으로 공간 상 설치 가능해야 합니다. (클릭, 단축키, UI 버튼 등 자유)
- Mirror는 Position / Rotation 조작이 가능해야 합니다. (Gizmo, 드래그, 단축키 등 자유)

## 4. 제출물
- 본 Git Repo를 Fork해서 본인 계정의 Git에서 수행 후 링크 공유.
- README를 통해 간단한 구조 설명 및 조작 방법 공유
</div>
</details>



---

# 구현 결과

## 조작 방법

**모드 전환**
| Edit 모드 | Play 모드 |
|---|---|
| ![Edit 모드](PreTest/Docs/Previews/preview-mode-edit.png) | ![Play 모드](PreTest/Docs/Previews/preview-mode-play.png) |
- 화면 우측 하단에 `Edit`/`Play` 버튼으로 전환. 좌측 상단에 현재 모드가 표시됨.
- `Play` 모드에서는 거울 선택과 Inspector 조회만 가능하고, `Edit` 모드에서는 배치·이동·회전·삭제까지 가능함.

**거울 배치 (Edit 모드)**
- `Add Mirror` 버튼 → 배치 모드 진입 → `Floor` 레이어 위 아무 곳이나 클릭하면 표면 법선에 맞춰 배치
- 배치 중 마우스 우클릭 또는 `Esc`로 취소.
- 최대 배치 개수에 도달하면 추가 배치가 불가능함.

**거울 조작 (Edit 모드)**
- 배치된 거울을 클릭하면 선택되고, 우측 Inspector 패널에 Name/Position/Rotation/Description이 표시됨 
- Inspector 패널에서 직접 입력 또는 기즈모 조작으로 Transform 조작 가능하며 양방향 동기화되어 있음.
- `viewport` 영역 좌측 상단 `Edit Option (Position/Rotation)` 버튼 조작 가능 (단축키 `W`/`E`)
- 이동·회전: 축별 핸들을 드래그 (Position-큐브, Rotation-링)
- 개별 삭제: Inspector 패널의 `Delete` 버튼, 키보드 `Delete` 키
- 전체 삭제: 우측 상단 `Clear` 버튼 (런타임에만 반영되며, 영구 반영하려면 `Save`를 눌러야 함).
- Inspector 패널은 하단 `Close` 버튼으로 닫고, 거울을 다시 클릭하면 열림.

| 예시 1 | 예시 2 |
|---|---|
| ![거울 배치 1](PreTest/Docs/Previews/preview-batch-mirror-1.gif) | ![거울 배치 2](PreTest/Docs/Previews/preview-batch-mirror-2.gif) |

**저장/불러오기**
- `Save` 버튼: 현재 배치(Position/Rotation/Name/Description)를 JSON으로 저장.
- 앱을 처음 실행하면 부트스트랩 씬(`Init`)이 저장된 배치를 읽고 게임 씬으로 진입 (저장 파일이 없거나 손상된 경우 빈 상태로 시작.)

**카메라**
- 회전: 마우스 우클릭 드래그
- 확대/축소: 스크롤
- 이동: 휠 버튼(가운데 클릭) 드래그
- 리셋: 좌측 상단 카메라 아이콘 버튼

![카메라 조작](PreTest/Docs/Previews/preview-camera-manipulation.gif)

**레이저**
- 초록색: Receiver에 도달
- 빨간색: 최대 반사 횟수까지 도달하고도 Receiver에 도달하지 못한 경우
- 노란색: 아직 반사 기회가 남아있는 상태

![레이저 색상 피드백](PreTest/Docs/Previews/preview-laser-color.gif)

**기타**
- 우측 상단 `Exit` 버튼: 애플리케이션 종료(에디터에서 테스트 중이면 Play 모드만 종료).

## 프로젝트 구조

```
Assets/Scripts/
├─ Laser/        레이저 시뮬레이션(LaserEmitter), 수신기(LaserReceiver), 반사/수신 인터페이스
├─ Mirror/       거울의 반사 로직(MirrorController, ILaserReflector 구현)
├─ Placement/    거울 배치·풀링(MirrorPlacementController, MirrorPool, PlacedMirror), 개수 표시
├─ Selection/    선택·기즈모 조작(MirrorSelectionController, MirrorGizmo), Inspector 패널
├─ Save/         JSON 저장/불러오기(SaveManager, LoadManager)
├─ Motions/      범용 DOTween 연출 컴포넌트(CanvasGroupAlphaLoop)
└─ (최상위)      GameManager, GameConfig, MonoSingleton, AppMode 등 앱 전역 매니저·설정
```

씬 구성
- `Init`: 항상 최초 실행되는 부트스트랩 씬으로 저장 데이터 로드 후 `Scene`으로 전환
- `Scene`: 실제 플레이 씬

## 아키텍처 설계

핵심 내용들을 정리하며, [로드맵](https://github.com/Seok-Min-Lee/PreTest/blob/main/PreTest/Docs/Roadmap.md)에는 무엇을 구현하는지, [구현 노트](https://github.com/Seok-Min-Lee/PreTest/blob/main/PreTest/Docs/ImplementationNotes.md) 에는 개발 과정이 자세히 기록되어 있습니다

1. **인터페이스 분리** — `ILaserReflector`(반사체가 정의하는 정면 벡터)와 `ILaserHitReceiver`(피격 콜백)에만 의존. 
2. **오브젝트 풀링** — `MirrorPool`이 활성된 오브젝트는 `List`, 비활성된 오브젝트는 `Queue`로 관리하며 재사용. 
3. **Raycast 기반 자유 배치** — 표면 법선 맞춤 도킹 및 Gizmo 수정 지원. 레이어 분리로 Raycast 간섭 방지.
4. **이벤트 기반 상태 전파** — 직접 참조 없는 발행/구독(Pub/Sub) 패턴을 적용하여 컴포넌트 간 결합도를 낮추고 인스턴스 의존성을 제거함.
5. **설정값 중앙화** — 주요 전역 설정값을 GameConfig(ScriptableObject)로 통합. Resources 기반 정적 싱글톤으로 로드하여 중앙 집중식 데이터 관리 및 일괄 수정 환경 구축.
6. **데이터 영속성** — `JSON` 포맷 기반 배치 데이터 입출력 구현 및 런타임-데이터 레이어 분리
7. **안전장치** — `ILaserReflector.ReflectiveNormal` 기반 앞/뒷면 판별, InputField 편집 중 키보드 단축키 오작동 방지 등.
