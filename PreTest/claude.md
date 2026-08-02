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