/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. https://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var fs = require("fs");
var sass = require("gulp-sass");
var cleanCSS = require('gulp-clean-css');
var sourcemaps = require('gulp-sourcemaps');
var npmFiles = require('gulp-npm-files');
var concatCSS = require('gulp-concat-css');

gulp.task("sass", function () {
    return gulp.src('Styles/*.scss')
        .pipe(sourcemaps.init())
        .pipe(sass())
        .pipe(concatCSS('bundle.css'))
        .pipe(cleanCSS())
        .pipe(sourcemaps.write())
        .pipe(gulp.dest('wwwroot/style'));
});
gulp.task('npm-dependencies', function () {
    gulp.src(npmFiles(), { base: './' }).pipe(gulp.dest('./wwwroot'));
});