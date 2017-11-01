/// <binding ProjectOpened='watch' />
module.exports = function(grunt) {
    grunt.initConfig({
        clean: {
            all: {
                src: [
                    "wwwroot/_/*"
                ]
            }
        },
        banner: '/*!\n' +
            ' * Lykke auth website - <%= grunt.template.today("yyyy-mm-dd") %>\n' +
            " * http://lykke.com/\n" +
            " */\n",
        less: {
            production: {
                options: {
                    paths: ["wwwroot/styles"],
                    sourceMap: true
                },
                files: {
                    "wwwroot/_/styles.css": "wwwroot/styles/styles.less"
                }
            }
        },
        concat: {
            css: {
                src: [
                    "node_modules/bootstrap/dist/css/bootstrap.css",
                    "wwwroot/styles/bootstrap-theme-lykke.css",
                    "wwwroot/_/styles.css"
                ],
                dest: "wwwroot/_/styles.css"
            },
            all: {
                src: [
                    "wwwroot/js/**/*.js",
                    "wwwroot/js/*.js"
                ],
                dest: "wwwroot/_/all.js"
            },
            vendor: {
                src: [
                    "node_modules/jquery/dist/jquery.js",
                    "node_modules/bootstrap/dist/js/bootstrap.js",
                    "node_modules/jquery-validation/dist/jquery.validate.js",
                    "node_modules/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js"
                ],
                dest: "wwwroot/_/vendor.js"
            }
        },
        cssmin: {
            css: {
                src: "wwwroot/_/styles.css",
                dest: "wwwroot/_/styles.min.css"
            }
        },
        uglify: {
            options: {
                banner: "<%= banner %>",
                sourceMap: true
            },
            all: {
                src: "<%= concat.all.dest %>",
                dest: "wwwroot/_/all.min.js"
            },
            vendor: {
                src: "<%= concat.vendor.dest %>",
                dest: "wwwroot/_/vendor.min.js"
            }
        },
        watch: {
            css: {
                files: ["wwwroot/styles/**/*.less"],
                tasks: ["less", "concat:css"],
                options: {
                    nospawn: true
                }
            },
            js: {
                files: [
                    "wwwroot/js/*.js",
                    "wwwroot/js/**/*.js"
                ],
                tasks: ["concat"]
            }
        }
    });

    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-watch");
    grunt.loadNpmTasks("grunt-contrib-less");

    grunt.registerTask("default", ["clean:all", "less", "concat", "cssmin", "uglify"]);
    grunt.registerTask("develop", ["clean:all", "less", "concat", "cssmin", "uglify"]);
    grunt.registerTask("all", ["clean:all", "less", "concat", "cssmin", "uglify"]);
};