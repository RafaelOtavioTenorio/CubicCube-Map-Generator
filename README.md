# CubicCube-Map-Generator
Large-Scale Earth-Like World Generator for Voxel 3D Games.

O Cubic³ Map Generator é um sistema de geração procedural de mundos desenvolvido especificamente para o jogo Cubic³, atualmente em desenvolvimento pela Momentum.
Seu objetivo é servir como a camada fundamental de world generation do jogo, permitindo a criação de mapas massivos, navegáveis e divididos em chunks, com continentes de escala earth-like, adequados para um jogo voxel 3D.

## Objetivo do projeto
O projeto foi criado para resolver um problema central do desenvolvimento do Cubic³:

Como gerar um mundo voxel extremamente grande, contínuo e explorável, sem depender exclusivamente de noise ou Wave Function Collapse, mantendo coerência geográfica em escala continental?

Para isso, o gerador atua em nível global, produzindo um mapa base determinístico que serve como referência para a geração de chunks voxel sob demanda.

## Pipeline de Geração:

O gerador opera através de uma pipeline central configurável, responsável por executar todas as etapas de geração do mundo.

### Opções de Saída da Pipeline

A pipeline principal oferece duas opções mutuamente exclusivas, configuráveis no ponto de entrada (Program.cs):

1. Exportação de imagem (Visualização)
* Gera imagens do mapa final (e mapas intermediários).
Utilizado para:
* Debug visual.
* Ajuste de parâmetros.
* Validação de continentes, relevo e clima.
* Usa conversão direta de dados para pixels.
➡️ Ideal durante desenvolvimento e balanceamento do gerador.

2. Exportação estrutural (Para UnityEngine)
* Exporta todos os mapas gerados como estruturas de dados.
* Projetado para importação direta na Unity Engine.
* Não depende de conversão para imagem.
* Mantém precisão total dos dados.
* Esses dados são consumidos diretamente pelo sistema de geração de chunks voxel do jogo.
* Este é o modo de produção, usado pelo Cubic³.

## Por que não usar apenas Noise ou WFC?

### Técnicas baseadas em Noise

* Excelente para variação local.
* Problemas em escala global:
* Falta de continentes bem definidos.
* Terrenos excessivamente caóticos.
* Navegação de longa distância pouco intuitiva.

### Wave Function Collapse

* Alto custo computacional.
* Escala mal para mundos grandes e contínuos.
* Pouco controle sobre macro-estrutura geográfica.

Para Cubic³, era necessário um sistema macro-estrutural, capaz de definir continentes, regiões e clima antes da geração voxel.

## Abordagem Técnica

### Projeção planar

A projeção planar foi escolhida para facilitar o uso do mapa na geração de chunks, também servindo como dados iniciais de minimizar posteriormente.
Foi considerada a geração já com distorção cúbica ou esférica, mas foi decidido planar pois permite mais liberdade criativa para os designers. Além disso, também é possível realizar a distorção facilmente na Unity Engine caso seja necessário.

A modelagem do mundo é realizada com listas de dados contendo:
* Altitude.
* Dados continentais.
* Temperatura.

Essa abordagem:
* Facilita indexação espacial.
* Escala bem para mapas grandes.
* Serve como base natural para chunking voxel.

## Continentes Earth-Like

### Massas Continentais Coesas
* Continentes gerados como blobs contínuos.

Controle explícito de:
* Escala.
* Distribuição.
* Densidade.

### Tectônica Simplificada

* Simulação de placas em escala macro.

Criação natural de:
* Cadeias montanhosas.
* Planícies.
* Fronteiras continentais.

O objetivo não é aleatoriedade visual, mas navegabilidade e identidade geográfica.

## Macro → Micro (World → Chunks)

O sistema é dividido em duas camadas bem definidas:

### Camada Global (Realizada nessa aplicação)
* Define continentes.
* Altitude média.
* Temperatura.
* Clima base.

### Camada Local
* Chunks voxel consultam o mapa global.
* Geração determinística por seed.
* Continuidade garantida entre regiões.

Essa separação permite:
* Geração sob demanda.
* Baixo custo de memória.

## Erosão e Suavização
Após a geração inicial:
* Algoritmos de erosão reduzem artefatos.
* Criam transições naturais.
* Melhoram jogabilidade voxel.

## Sistema de Temperatura
A temperatura é calculada com base em:
* Latitude.
* Altitude.

Esses dados influenciam:
* Biomas futuros.
* Variação climática.
* Geração voxel contextual.
