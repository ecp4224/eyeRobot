import setuptools

with open("README.md", "r") as fh:
    long_description = fh.read()

setuptools.setup(
    name="eyerobot_openai",
    version="0.0.1",
    author="Eddie Penta",
    author_email="ecp4224@gmail.com",
    description="An OpenAI gym that implements the path finding logic for the EyeRobot",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/edkek/eyerobot",
    packages=setuptools.find_packages(),
    install_requires=['gym'],
    classifiers=(
        "Programming Language :: Python :: 3",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
    ),
)