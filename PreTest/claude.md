## 기본 언어 설정
- 모든 설명, 답변, 주석은 한국어로 작성할 것.

## 명칭 규칙
| 구분 | 스타일 | 예시 |
| :--- | :--- | :--- |
| **비공개 필드** | `_camelCase` | `private float _moveSpeed;` |
| **공개 멤버** | `PascalCase` | `public void TakeDamage();` |
| **타입 명시** | 명시적 타입 사용 | `var` 사용 자제 (Primitive 타입 제외) |

## 구체적 코딩 규칙
- **접근 지정자:** 모든 선언 시 접근 지정자(`public`, `private` 등)를 생략하지 않고 명시
- **제어문:** 1줄의 조건문/반복문이라도 반드시 중괄호 `{ }` 사용
- **로직 구조:** 중첩 구조 최소화 및 `Early Return` 패턴 최우선 적용

## Unity 성능 및 라이프사이클 규칙
- `MonoBehaviour` 상속 클래스는 C# 생성자 대신 `Awake()`, `Start()`, `Reset()` 사용
- `GetComponent` 호출 최소화 (`Awake()` 또는 `Start()`에서 미리 캐싱)
- 불필요한 GC(가비지 컬렉션) 발생 방지 (`Update()` 내에서 `new` 객체 생성 금지, 문자열 결합 지양)

## Unity UI 이벤트 연결 규칙
- `Button.onClick`, `TMP_InputField.onEndEdit` 등 UnityEvent는 코드에서 `AddListener`로 구독하지 말고, 씬 Inspector의 퍼시스턴트 콜로 직접 연결
- 핸들러는 `public void OnClickX()` / `public void OnEndEditX(string value)` 형태의 공개 메서드로 작성 (전용 `Button`/`InputField` 필드를 따로 들고 있을 필요 없음, 값 표시·상호작용 토글 등 다른 이유로 참조가 필요할 때만 필드 유지)
- `OnEnable`/`OnDisable` 등 코드 구독은 순수 C# 이벤트(`ModeChanged`, `SelectionChanged` 등)에만 사용