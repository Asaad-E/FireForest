#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;

out vec4 finalColor;

// Bloom settings
const float intensity = 1.2; 

// Targeted RGB color (115, 2, 19) normalized
const vec3 targetColor = vec3(115.0 / 255.0, 2.0 / 255.0, 19.0 / 255.0); 

// Color similarity tolerance
const float colorTolerance = 0.30; 

void main()
{
    vec4 source = texture(texture0, fragTexCoord) * fragColor * colDiffuse;
    vec3 bloomSum = vec3(0.0);
    float passCount = 0.0;

    int samples = 4;
    float quality = 8;
    vec2 texelSize = quality / vec2(800.0, 600.0); 

    for (int x = -samples; x <= samples; x++)
    {
        for (int y = -samples; y <= samples; y++)
        {
            vec2 offset = vec2(float(x), float(y)) * texelSize;
            vec4 texel = texture(texture0, fragTexCoord + offset) * fragColor * colDiffuse;

            float dist = distance(texel.rgb, targetColor);

            if (dist < colorTolerance)
            {
                float matchFactor = 1.0 - (dist / colorTolerance);
                bloomSum += texel.rgb * matchFactor;
                passCount += 1.0;
            }
        }
    }

    if (passCount > 0.0)
    {
        bloomSum /= passCount;
    }

    // 1. Calculate underlying pixel brightness (0.0 for pure black)
    float baseBrightness = max(source.r, max(source.g, source.b));

    // 2. Smoothstep creates a clean cutoff near black to kill dark noise
    float blackMask = smoothstep(0.05, 0.2, baseBrightness);

    // 3. Mask the bloom sum so black/dark areas receive zero glow
    vec3 maskedBloom = bloomSum * intensity * blackMask;

    finalColor = vec4(source.rgb + maskedBloom, source.a);
}