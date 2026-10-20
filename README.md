# bootplus2026
청년도약인재양성부트캠프 중급 AI 활용게임개발

## Slime Hunter RPG

Unity 6 기반 3D 슬라임 사냥 게임입니다. 레벨 1~3의 슬라임을 처치한 뒤 거대 슬라임 보스를 잡으면 게임을 클리어합니다.

### 프로젝트 열기

1. 저장소를 복제합니다.
2. Unity Hub에서 프로젝트 폴더를 추가합니다.
3. **Unity 6000.5.7f1**로 열고 패키지 설치 및 에셋 임포트가 끝날 때까지 기다립니다.
4. `Assets/Scenes/SampleScene.unity`를 열어 실행합니다.

SpringBone 로컬 패키지는 `Packages/com.unity.springbone`에 포함되어 있습니다.

### 조작

| 키 | 동작 |
| --- | --- |
| WASD | 이동 |
| Shift | 달리기 |
| Space | 바라보는 방향으로 공격 |
| E | 가까운 HP 물약 먹기 |
| ? 버튼 | 조작 안내 열기 |
| ESC | Windows: 프로그램 종료 / Web: GAME OVER |

몬스터 처치 시 HP 30을 회복하고, 25% 확률로 떨어지는 물약을 먹으면 HP 50을 추가 회복합니다. 사망 시 다시 시작 또는 게임 종료를 선택할 수 있습니다.

### 빌드

- **Windows**: Build Profiles에서 Windows x64를 선택하고 빌드합니다. EXE와 데이터 폴더·DLL을 함께 배포해야 합니다.
- **Web / itch.io**: Web Build Support 모듈을 설치한 뒤 Web 플랫폼을 선택합니다. Gzip 압축과 Decompression Fallback을 사용하며, 생성된 `index.html`이 ZIP 최상위에 오도록 압축합니다.
- Web 빌드는 파일을 직접 열지 말고 HTTP 서버나 itch.io에서 실행합니다.

`Library`, `Temp`, `Logs`, `Builds` 등 생성 파일과 개인 도구 설정은 Git에서 제외됩니다. 에셋의 라이선스 파일과 나눔고딕 OFL은 프로젝트에 포함되어 있습니다.
