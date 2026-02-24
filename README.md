# CubicCube-Map-Generator
Large-Scale Earth-Like World Generator for Voxel 3D Games

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
