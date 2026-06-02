# Tasty Meal Planner (FoodDrinkApp)

Tasty Meal Planner is a premium, cross-platform mobile application built with **.NET MAUI**. It is designed to offer a magazine-style, visually stunning experience for discovering, tracking, and managing your daily meals and nutritional goals.

## Key Features

- **Meal Tracking & Collection**: Log your foods and drinks with rich metadata including calories, macros (protein, carbs, fat), and allergy notes.
- **Diet Dashboard**: Track your daily calorie intake, monitor macro progress with dynamic charts, and view today's summary.
- **Profile Management**: Set up a personalized profile with your age, height, weight, activity level, and goals to automatically calculate your daily calorie needs.
- **Modern UI/UX**: Enjoy a highly polished interface with custom fonts (Playfair Display, FluentIcons), smooth animations, unified Floating Action Buttons (FABs), and dark mode support.
- **Accessibility (a11y) First**: Built-in "Large Text" mode dynamically scales text sizes across all UI components, and Text-to-Speech (TTS) reads nutrition facts aloud.
- **Cloud Sync**: Dual-write strategy integrating with a backend MockAPI for persistent storage, with seamless offline local fallback.

## Hardware Integration (Core Feature)

This application heavily leverages native device hardware capabilities to provide an immersive and interactive user experience.

- **Camera**: Capture profile avatars and food pictures directly from the app using the device camera.
- **Geolocation**: Tag your meals or profile with your exact geographic coordinates. The app accesses native GPS hardware to retrieve latitude and longitude.
- **Haptics & Vibration**: Provides rich tactile feedback. The device's vibration motor is triggered during validation errors (e.g., trying to remove a non-existent photo) or hardware testing to ensure robust user interactions.
- **Text-to-Speech (TTS)**: Leverages native device accessibility engines to read food details, ingredients, and nutrition facts aloud for visually impaired users.
- *(Note: A dedicated `HardwarePage` is included in the app to run real-time diagnostics on Camera, GPS, and Haptics).*

## Project Structure

```text
FoodDrinkApp/
├── Models/              # Data structures and business entities
│   ├── FoodItem.cs      # Core food data model (macros, images, categories)
│   └── UserProfile.cs   # User profile and calorie calculation logic
├── Views/               # XAML UI Pages and Code-behinds
│   ├── MainPage.xaml         # Home catalog with categorized food lists
│   ├── FoodDetailPage.xaml   # Hero-style detailed view of a food item
│   ├── AddItemPage.xaml      # Form to create new food entries
│   ├── ProfilePage.xaml      # User dashboard and calorie progress
│   ├── EditProfilePage.xaml  # Profile creation and hardware interactions
│   ├── DietRecordPage.xaml   # Daily consumption summary
│   ├── HardwarePage.xaml     # Hardware testing & diagnostics suite
│   └── SettingsPage.xaml     # Theme and Accessibility toggles
├── Services/            # Core business logic and API integrations
│   ├── FoodCatalogService.cs # Cloud (MockAPI) & Local data sync
│   ├── DietRecordService.cs  # Diet tracking state management
│   ├── AccessibilityService.cs # Dynamic font scaling logic
│   └── SpeechService.cs      # Native TTS wrapper
├── Resources/           # Assets, AppIcons, Splash, Fonts, and Styles
│   ├── Fonts/           # Playfair Display & Fluent System Icons
│   └── Styles/          # Global color palettes and dark/light themes
└── MauiProgram.cs       # App bootstrapping and dependency injection
```

## Supported Platforms

As a .NET MAUI application, the project natively supports the following platforms:
- **Android** (Target API 34+ recommended. Requires `CAMERA`, `VIBRATE`, and `ACCESS_COARSE_LOCATION` permissions)
- **iOS** (Requires corresponding `Info.plist` hardware usage descriptions)
- **Windows** (WinUI 3)
- **Mac Catalyst**

## How to Run

### Prerequisites
- Install **Visual Studio 2022** (17.8 or later) or **Visual Studio Code** with the .NET MAUI extension.
- Ensure the **.NET 8.0 SDK** (or later) and the **.NET MAUI workload** are installed.
- Android Emulator / Physical Android Device (with Developer Mode enabled).

### Running the App
1. Clone the repository to your local machine.
2. Open `FoodDrinkApp.sln` in Visual Studio.
3. In the toolbar, select your target device (e.g., `Pixel 5 - API 34 (Android 14.0)`).
4. Click the **Run** (Play) button or press `F5` to build and deploy the app.
5. *Note for Hardware features*: To fully test the Camera, GPS, and Haptic feedback, it is highly recommended to run the app on a **physical device**. Emulators may only provide mock location or simulated camera feeds.
