#version 330 core

#include "light.glsl"
#include "fresnel.glsl"
#include "diffuse.glsl"
#include "specular.glsl"

#define MAX_LIGHT 10
#define PI 3.14159265359

// Adicione luz especular ao modelo de sombreamento

in vec3 worldPosition;
in vec3 worldNormal;

out vec4 FragColor;

uniform Light lights[MAX_LIGHT];
uniform vec3 cameraPos;      // Posição da câmera no espaço mundo
uniform vec3 ambientColor;   // Cor da luz ambiente

void main()
{
    // Normaliza o vetor normal do fragmento
    vec3 worldNormalNormalized = normalize(worldNormal);

    // Vetor da direção de visualização (do fragmento para a câmera)
    vec3 viewDirection = normalize(cameraPos - worldPosition);

    // Propriedades do material (ajustáveis)
    vec3 baseColor = vec3(0.5, 0.2, 0.5);
    float metallic = 0.0;
    float roughness = 0.25;

    // F0 (refletância em incidência normal) – 0.04 para dielétricos, ou a cor base para metais
    vec3 F0 = mix(vec3(0.04), baseColor, metallic);

    // Acumulador de cor
    vec3 color = vec3(0.0);

    // Loop sobre todas as luzes (máximo MAX_LIGHT)
    for (int i = 0; i < MAX_LIGHT; i++)
    {
        Light light = lights[i];
        if (light.type == LIGHT_UNSET)
            break;

        // Dados da luz
        float attenuation = computeLightAttenuation(light, worldPosition);
        vec3 lightColor = light.color * light.intensity;
        vec3 lightDirection = computeLightDirection(light, worldPosition);

        // Half‑angle (vetor médio entre direção da luz e direção de visão)
        vec3 halfAngle = normalize(lightDirection + viewDirection);

        // Fresnel (Schlick)
        float VdotH = max(dot(viewDirection, halfAngle), 0.0);
        vec3 fresnel = F0 + (1.0 - F0) * pow(1.0 - VdotH, 5.0);

        // Difusa (Lambert) – com correção de energia: (1 - metallic) * baseColor / PI
        vec3 diffuse = (1.0 - metallic) * baseColor / PI;

        // Especular (Blinn‑Phong puro, sem Fresnel)
        vec3 specular = specularReflectance(fresnel, worldNormalNormalized, halfAngle,
                                            viewDirection, lightDirection, roughness);

        // Refletância final: combinação energética (1 - F) * diffuse + F * specular
        vec3 reflectance = (1.0 - fresnel) * diffuse + fresnel * specular;

        // Termo N·L (difuso/lambertiano)
        float NdotL = max(dot(worldNormalNormalized, lightDirection), 0.0);

        // Contribuição da luz atual
        vec3 lightContribution = reflectance * lightColor * NdotL * attenuation;
        color += lightContribution;
    }

    // Adiciona a iluminação ambiente global
    color += ambientColor * baseColor;

    // Atribui a cor final (alfa = 1.0)
    FragColor = vec4(color, 1.0);
}
