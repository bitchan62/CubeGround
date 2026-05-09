# CubeGround
CubeGround 유니티 C# 기반 게임

Unity를 활용한 동적 환경 구축 및 오브젝트 제어 시스템 프로젝트입니다. 
이 프로젝트는 주로 지형의 동적인 변화에 따른 캐릭터의 이동 및 시스템 설정을 관리하는 데 중점을 둡니다.

## 📌 프로젝트 개요
- **플랫폼**: PC / Unity Engine
- **주요 기술**: Unity Navigation (NavMesh), C# Scripting, Physics System
- **핵심 목표**: 움직이는 플랫폼(Cube) 상에서의 안정적인 경로 탐색 및 물리 상호작용 구현

## 🛠 기술 스택
- **Engine**: Unity 2021.x 이상 (권장)
- **Language**: C#
- **Version Control**: Git

## 📂 주요 파일 및 폴더 구조
프로젝트 운영에 필요한 핵심 설정 파일들입니다.

*   **Packages/**: 프로젝트에 의존성으로 추가된 유니티 패키지(예: AI Navigation, TextMeshPro 등)의 리스트와 매니페스트 파일이 포함되어 있습니다.
*   **ProjectSettings/**: 
    *   `TagManager.asset`: 프로젝트에서 사용되는 태그와 레이어 설정.
    *   `Physics2DSettings.asset` / `DynamicsManager.asset`: 물리 엔진 설정.
    *   `EditorBuildSettings.asset`: 빌드에 포함된 씬(Scene) 리스트 관리.
*   **UserSettings/**: 사용자의 로컬 에디터 설정 (레이아웃, 마지막 작업 씬 등).
*   **.gitignore**: Unity 프로젝트에서 불필요한 캐시 및 임시 파일(`Library/`, `Temp/`, `Logs/` 등)을 제외하기 위한 설정 파일.

## 🚀 시작하기
1. **Repository 클론**:
   ```bash
   git clone [https://github.com/bitchan62/CubeGround.git](https://github.com/bitchan62/CubeGround.git)
