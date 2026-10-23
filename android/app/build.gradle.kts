import java.util.Properties

plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    alias(libs.plugins.kotlin.compose)
}

val localProperties = Properties()
val localPropertiesFile = rootProject.file("local.properties")
if (localPropertiesFile.exists()) {
    localPropertiesFile.inputStream().use { localProperties.load(it) }
}

fun normalizeHttpBase(raw: String?): String {
    if (raw.isNullOrBlank()) return ""
    var url = raw.trim().trimEnd('/')
    if (!url.startsWith("http://", ignoreCase = true) && !url.startsWith("https://", ignoreCase = true)) {
        url = "http://$url"
    }
    return url
}

fun sanitizeDevHost(raw: String?): String {
    val fallback = "10.0.2.2"
    if (raw.isNullOrBlank()) return fallback
    var host = raw.trim().removePrefix("/")
    if (host.startsWith("http://", ignoreCase = true)) host = host.removePrefix("http://")
    if (host.startsWith("https://", ignoreCase = true)) host = host.removePrefix("https://")
    host = host.substringBefore("/").substringBefore(":").trim()
    return host.ifBlank { fallback }
}

val useAdbReverse: Boolean =
    localProperties.getProperty("dev.useAdbReverse")?.trim()?.equals("true", ignoreCase = true) == true
val devHost: String =
    if (useAdbReverse) "127.0.0.1" else sanitizeDevHost(localProperties.getProperty("dev.host"))
val bffBaseUrl: String = normalizeHttpBase("$devHost:5100")
val keycloakBaseUrl: String =
    normalizeHttpBase(localProperties.getProperty("dev.keycloak.publicUrl"))
        .ifBlank { "http://$devHost:8080" }

android {
    namespace = "com.arrowpuzzle.app"
    compileSdk = 35

    defaultConfig {
        applicationId = "com.arrowpuzzle.app"
        minSdk = 26
        targetSdk = 35
        versionCode = 30
        versionName = "0.2.20-server-progress"

        buildConfigField("String", "BFF_BASE_URL", "\"$bffBaseUrl\"")
        buildConfigField("String", "KEYCLOAK_BASE_URL", "\"$keycloakBaseUrl\"")
        buildConfigField("String", "OIDC_REALM", "\"arrow-puzzle\"")
        buildConfigField("String", "OIDC_ANDROID_CLIENT_ID", "\"arrow-puzzle-android\"")
        buildConfigField("String", "OIDC_DEV_CLIENT_ID", "\"arrow-puzzle-dev-cli\"")
        buildConfigField("String", "BUILD_TIME", "\"${System.currentTimeMillis()}\"")
    }

    buildFeatures {
        compose = true
        buildConfig = true
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    kotlinOptions {
        jvmTarget = "17"
    }

    packaging {
        resources {
            excludes += "/META-INF/{AL2.0,LGPL2.1}"
        }
    }
}

val adbExecutable: String = run {
    val sdk = localProperties.getProperty("sdk.dir")?.replace("\\\\", "\\")?.trim().orEmpty()
    if (sdk.isNotEmpty()) {
        val candidate = file("$sdk/platform-tools/adb.exe")
        if (candidate.isFile) candidate.absolutePath else "adb"
    } else {
        "adb"
    }
}

tasks.register("adbReverseBff") {
    doLast {
        exec {
            commandLine(adbExecutable, "reverse", "tcp:5100", "tcp:5100")
            isIgnoreExitValue = true
        }
    }
}

afterEvaluate {
    tasks.findByName("installDebug")?.finalizedBy("adbReverseBff")
}

dependencies {
    implementation(libs.androidx.core.ktx)
    implementation(libs.androidx.lifecycle.runtime.ktx)
    implementation(libs.androidx.lifecycle.viewmodel.compose)
    implementation(libs.androidx.activity.compose)
    implementation(platform(libs.androidx.compose.bom))
    implementation(libs.androidx.ui)
    implementation(libs.androidx.ui.graphics)
    implementation(libs.androidx.ui.tooling.preview)
    implementation(libs.androidx.material3)
    implementation(libs.retrofit)
    implementation(libs.retrofit.moshi)
    implementation(libs.okhttp)
    implementation(libs.okhttp.logging)
    implementation(libs.moshi)
    implementation("androidx.browser:browser:1.8.0")

    debugImplementation("androidx.compose.ui:ui-tooling")

    testImplementation("junit:junit:4.13.2")
}
