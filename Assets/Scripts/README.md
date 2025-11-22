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
     - Tick Rate: 1.0 (일정한 틱 속도)

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
- **좌클릭 (홀드)**: 총 발사 - 투사체가 날아가 맞은 세포를 파괴
- **우클릭**: 세포 생성 (Write) - 빈 공간에만 가능, 적의 과밀 유도용

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
│   └── GameOfLifeManager.cs # 게임 규칙, 틱 시스템, 미로 생성
├── Visual/
│   └── CellVisualizer.cs    # 세포 시각화
└── Player/
    ├── PlayerController.cs  # 플레이어 이동, 총 발사
    └── Projectile.cs        # 투사체 (총알)
```

## 초기 패턴 - 플랫포머 미로

게임 시작 시 자동으로 생성되는 플랫포머 스타일 미로:
- **벽과 바닥**: 미로의 외곽 경계
- **플랫폼들**: 여러 층의 수평 플랫폼
- **계단**: 좌우에 올라가는/내려가는 계단
- **중앙 장애물**: 박스 형태의 장애물
- **동적 패턴**: Glider, Blinker 등이 미로 내부에서 활동

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
