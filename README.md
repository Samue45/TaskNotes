# 🗂️ TaskNotes

### Aplicación de gestión de tareas con interfaz natural en .NET MAUI (Windows)

------------------------------------------------------------------------

## 📖 Introducción

**TaskNotes** es una aplicación de escritorio desarrollada con **.NET
MAUI**, orientada a la plataforma **Windows**, cuyo objetivo es
facilitar la gestión de tareas personales mediante una interfaz clara,
estructurada y apoyada en el uso de **interfaces naturales**,
concretamente el **reconocimiento de voz**.

Este proyecto ha sido desarrollado como **aplicación libre** dentro del
marco del\
**Tema 5 -- Interfaces Naturales en .NET MAUI**, cumpliendo los
requisitos establecidos para la entrega.

------------------------------------------------------------------------

## 🎯 Objetivos del proyecto

-   Desarrollar una aplicación funcional utilizando **.NET MAUI**.
-   Aplicar el patrón arquitectónico **MVVM**.
-   Diseñar una interfaz de usuario cuidada y coherente.
-   Integrar una **interfaz natural basada en voz**.
-   Implementar navegación básica entre pantallas.
-   Cumplir los requisitos técnicos y funcionales del tema.

------------------------------------------------------------------------

## 🖥️ Plataforma objetivo

-   **Sistema operativo:** Windows\
-   **Tipo:** Aplicación de escritorio\
-   **Entorno de desarrollo:** Visual Studio 2022 o superior

La aplicación está configurada y probada para ejecutarse correctamente
en **Windows**, cumpliendo el requisito mínimo de ejecución.

------------------------------------------------------------------------

## ✅ Cumplimiento de requisitos --- Tema 5

### 1️⃣ Tecnología base

-   ✔ Aplicación desarrollada en **.NET MAUI**\
-   ✔ Ejecutable en **Windows**

------------------------------------------------------------------------

### 2️⃣ Funcionalidad mínima

La aplicación incluye:

-   ✔ **Navegación básica**
    -   Uso de `NavigationPage`
-   ✔ **Al menos dos páginas**
    -   Página principal con listado de tareas\
    -   Página de creación y edición de tareas
-   ✔ **Interfaz cuidada**
    -   Diseño basado en tarjetas\
    -   Jerarquía visual clara\
    -   Soporte para modo claro y oscuro
-   ✔ **Controles comunes**
    -   `Entry`
    -   `Editor`
    -   `Button`
    -   `CollectionView`
    -   `Picker`
    -   `DatePicker`
    -   `CheckBox`

------------------------------------------------------------------------

### 3️⃣ Interfaz natural obligatoria --- Voz 🗣️

La aplicación integra **reconocimiento de voz (Speech-to-Text)**
mediante\
`CommunityToolkit.Maui.Media`.

Funciones implementadas:

-   ✔ Solicitud de permisos de micrófono\
-   ✔ Captura de audio\
-   ✔ Conversión de voz a texto\
-   ✔ Creación de tareas mediante dictado\
-   ✔ Gestión segura del ciclo de grabación

El uso de la voz permite una interacción más natural y reduce la
necesidad de entrada manual.

------------------------------------------------------------------------

## 🧠 Arquitectura

TaskNotes sigue el patrón **MVVM (Model--View--ViewModel)**,
favoreciendo una correcta separación de responsabilidades y un código
mantenible.

-   **Models:** Definen las entidades y los datos.
-   **ViewModels:** Contienen la lógica de negocio y el estado.
-   **Views:** Definen la interfaz de usuario mediante XAML.
-   **Converters:** Adaptan valores del ViewModel a la interfaz.

La gestión de tareas se basa en una **única fuente de la verdad**,
evitando inconsistencias al aplicar filtros u ordenaciones.

    TaskNotes/
    ├── MVVM/
    │ ├── Models/
    │ ├── ViewModels/
    │ ├── Views/
    │ └── Converters/
    ├── Resources/
    ├── Platforms/Windows/
    └── TaskNotes.csproj

------------------------------------------------------------------------

## ✨ Funcionalidades principales

-   Crear, editar y eliminar tareas
-   Marcar tareas como completadas
-   Asignar prioridades (Alta, Media, Baja)
-   Filtrar y ordenar tareas
-   Crear tareas mediante reconocimiento de voz
-   Feedback visual y estados informativos

------------------------------------------------------------------------

## 🛠️ Tecnologías utilizadas

-   .NET MAUI\
-   CommunityToolkit.Maui\
-   CommunityToolkit.Maui.Media\
-   CommunityToolkit.Mvvm\
-   XAML\
-   Inyección de dependencias\
-   Patrón MVVM

------------------------------------------------------------------------

## 🚀 Ejecución del proyecto

1.  Clonar el repositorio:

    ``` bash
    git clone https://github.com/tu-usuario/tasknotes.git
    ```

2.  Abrir `TaskNotes.slnx` con Visual Studio 2022 o superior

3.  Seleccionar **Windows** como plataforma

4.  Ejecutar la aplicación

------------------------------------------------------------------------

## 👤 Autor

**Samuel Ayllón Sevilla**\

------------------------------------------------------------------------
