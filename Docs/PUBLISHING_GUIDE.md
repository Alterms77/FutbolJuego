# FutbolJuego – Publishing Guide

## Unity Build Settings

### Android
- Scripting Backend: IL2CPP
- Target Architectures: ARMv7 + ARM64
- Minify: Release → ProGuard
- Target API: 34, Min API: 26
- Keystore: sign with dedicated release keystore (keep in password manager)

### iOS
- Bundle ID: `com.futboljuego.manager`
- Minimum iOS: 14.0
- Enable Bitcode: No (Firebase requirement)
- Capabilities: Push Notifications, Sign In with Apple

## Firebase Setup
1. Create project at console.firebase.google.com
2. Add Android app (package name) → download `google-services.json` → `Assets/`
3. Add iOS app (bundle ID) → download `GoogleService-Info.plist` → `Assets/`
4. Enable Authentication → Email/Password + Google Sign-In
5. Create Firestore database in production mode
6. Deploy security rules: `firebase deploy --only firestore:rules`
7. Deploy Cloud Functions: `cd Backend/functions && npm install && firebase deploy --only functions`

## Google Play Submission Checklist
- [ ] Signed APK/AAB with release keystore
- [ ] Target API Level ≥ 34
- [ ] Privacy Policy URL in store listing
- [ ] Data Safety form completed
- [ ] Content rating questionnaire (Everyone / PEGI 3)
- [ ] Store screenshots: phone (min 2), tablet (recommended)
- [ ] Feature graphic 1024×500
- [ ] Short description ≤ 80 chars, full description ≤ 4 000 chars

## Apple App Store Submission Checklist
- [ ] Provisioning profile + distribution certificate
- [ ] Archive in Xcode → validate → upload via Transporter
- [ ] App Privacy labels completed
- [ ] Age rating: 4+
- [ ] Screenshots: 6.5" (iPhone 14 Pro Max), 5.5" (iPhone 8 Plus), 12.9" iPad
- [ ] Review notes: explain any in-app purchases

## Monetisation Policy Compliance
- All IAP must be disclosed in Privacy Policy
- COPPA: game rated 4+/PEGI 3, no data collection from under-13s
- Google Play Billing Library required for Android IAP
- Apple StoreKit 2 required for iOS IAP
