# Conway's Game of Life - POC

콘웨이의 생명 게임을 기반으로 한 탑다운 생존 액션 게임 POC입니다.

## 게임 컨셉

- **배경:** 검은색 (Empty Space)
- **적(시스템):** 형광 흰색 사각형 (Live Cell)
- **플레이어:** 붉은색 픽셀 (Glitch)
- **예측 UI:** 희미한 회색 (다음 턴에 생겨날/죽을 세포 미리보기)

## 설치 방법

### 1. 씬 설정

1. Unity에서 `Assets/Scenes/Game Of Life.unity` 씬을 엽니다.

2. **=== Manager ===** 오브젝트에 컴포넌트 추가:
   - `GameOfLifeManager` 스크립트 추가
   - 설정:
     - Grid Width: 50
     - Grid Height: 50
     - Cell Size: 1
     - Initial Tick Rate: 1.0
     - Min Tick Rate: 0.5

3. **=== Object ===** 하위에 새 오브젝트 생성:
   - 이름: `CellVisualizer`
   - `CellVisualizer` 스크립트 추가
   - Game Manager 필드에 Manager 오브젝트의 `GameOfLifeManager` 드래그

4. **=== Object ===** 하위에 새 오브젝트 생성:
   - 이름: `Player`
   - `PlayerController` 스크립트 추가
   - `SpriteRenderer` 추가 (자동으로 추가됨)
   - 스프라이트 설정:
     - Sprite: Unity 기본 스프라이트 (Square 또는 Circle)
     - Color: 빨간색 (Red)
     - Scale: (0.5, 0.5, 1)
   - Game Manager 필드에 Manager 오브젝트의 `GameOfLifeManager` 드래그

5. **Main Camera** 설정:
   - Background Color: 검은색 (0, 0, 0)
   - Orthographic Size: 15 (그리드가 잘 보이도록 조정)

### 2. 프로젝트 설정

Unity Input Manager 설정 확인:
- Edit > Project Settings > Input Manager
- Horizontal, Vertical 축이 설정되어 있는지 확인 (기본값 사용)

## 조작법

- **WASD / 화살표 키**: 플레이어 이동
- **좌클릭 (홀드)**: 세포 삭제 (Delete)
- **우클릭**: 세포 생성 (Write) - 빈 공간에만 가능

## 게임 규칙

### 콘웨이의 생명 게임 규칙

1. **고립 (죽음)**: 주변에 세포가 1개 이하면 죽습니다
2. **과밀 (죽음)**: 주변에 세포가 4개 이상이면 죽습니다
3. **탄생 (증식)**: 빈칸 주변에 세포가 정확히 3개 있으면 새 세포가 태어납니다

### 플레이어

- 적(흰색 세포)과 부딪히면 데미지를 입습니다
- 체력: 3
- 무적 시간: 1초 (데미지 후)
- 체력이 0이 되면 게임 오버

## 스크립트 구조

```
Assets/Scripts/
├── Core/
│   ├── Cell.cs              # 세포 데이터 클래스
│   └── GridManager.cs       # 격자 시스템 관리
├── Manager/
│   └── GameOfLifeManager.cs # 게임 규칙 및 틱 시스템
├── Visual/
│   └── CellVisualizer.cs    # 세포 시각화
└── Player/
    └── PlayerController.cs  # 플레이어 이동 및 입력
```

## 초기 패턴

게임 시작 시 자동으로 생성되는 패턴:
- **Glider** (5, 5): 대각선으로 움직이는 패턴
- **Blinker** (15, 15): 수직/수평으로 진동하는 패턴
- **Block** (25, 25): 정적인 패턴

## 디버깅

- Scene 뷰에서 그리드 선이 회색으로 표시됩니다
- Console에서 플레이어 데미지 로그 확인 가능
- Inspector에서 실시간으로 Tick Rate 확인 가능

## 다음 단계

POC 확장 아이디어:
1. 커널(탈출구) 시스템 추가
2. 더 복잡한 적 패턴 (Glider Gun, Pulsar 등)
3. 플레이어 능력 (슬로우 모션, 범위 공격 등)
4. UI 시스템 (체력, 스코어, 타이머)
5. 사운드 효과 및 배경음악
