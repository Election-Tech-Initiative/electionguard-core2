# AngularDemo

In order to use the electionguard package in angluar, you need to tell angular to use a custom webpack build. [Digital Ocean has some nice instructions](https://www.digitalocean.com/community/tutorials/angular-custom-webpack-config) but the process is simple:
1. load `@angular-builders/custom-webpack:browser`
2. modify your angular.json file builder to use the custom config
3. modify your angular.json file server to serve the custom config in dev mode.

## Angular

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 15.2.6.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via a platform of your choice. To use this command, you need to first add a package that implements end-to-end testing capabilities.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.io/cli) page.
